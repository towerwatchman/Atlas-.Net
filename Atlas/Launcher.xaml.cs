using Atlas.Core;
using Atlas.Core.Database;
using Atlas.Core.Networking;
using Atlas.Core.Utilities;
using Atlas.UI;
using Newtonsoft.Json.Linq;
using NLog;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Atlas
{
    public partial class Launcher : Window
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Launcher()
        {
            InitializeComponent();
            //Load Settings First
            Settings.Init();

            //Show debug console if it is enabled
            if (Atlas.Core.Settings.Config.ShowDebugWindow == false)
            {
                ConsoleManager.Hide();
            }
            else
            {
                ConsoleManager.Show();
            }

            Logger.Info($"Loaded settings from: {System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.ini")}");

            //Assign progressbar to helper
            InterfaceHelper.LauncherWindow = this;
            InterfaceHelper.LauncherProgressBar = LauncherProgressBar;
            InterfaceHelper.UpdateProgressBar = UpdateProgressBar;
            InterfaceHelper.LauncherTextBox = LauncherTextBox;
            InterfaceHelper.UpdateTextBox = UpdateTextBox;

            //Hide update progress bar
            UpdateProgressBar.Visibility = Visibility.Hidden;
            UpdateTextBox.Visibility = Visibility.Hidden;

            //This will be the default dispatcher for all UI elements that need to be updated from another thread
            //This does not include the Launcher. It uses its own thread.
            InterfaceHelper.Dispatcher = Application.Current.Dispatcher;

            //Check if program is already open
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1) System.Diagnostics.Process.GetCurrentProcess().Kill();

            //Fix context menu
            var menuDropAlignmentField = typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            Action setAlignmentValue = () =>
            {
                if (SystemParameters.MenuDropAlignment && menuDropAlignmentField != null) menuDropAlignmentField.SetValue(null, false);
            };
            setAlignmentValue();
            SystemParameters.StaticPropertyChanged += (sender, e) => { setAlignmentValue(); };

            Task.Run(async () =>
            {
                try
                {
                    //Run all tasks prior to opening
                    await Init();
                    //Launch Main Window
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        LaunchMainWindow();
                   });

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });

        }

        public async Task Init()
        {
            Logger.Info("Checking for Updates");

            UpdateLauncherText("Checking For Updates");
            //Check for Program updates
            await Task.Run(async () =>
            {
                try
                {
                    await Updater.CheckForUpdatesAsync();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });
            UpdateLauncherProgressBar(5);

            Logger.Info("Updating Folders");
            UpdateLauncherText("Updating Folders");
            //Set folders
            try
            {
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data"));
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data", "games"));
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data", "images"));
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data", "logs"));
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data", "updates"));
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes"));
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "ui"));
                Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "banners"));
            }
            catch (Exception ex) { Logger.Error(ex); }
            UpdateLauncherProgressBar(10);

            Logger.Info("Updating xaml Dependencies");
            UpdateLauncherText("Updating xaml Dependencies");

            //Set the default theme file

            try
            {
                string theme = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "ui", $"{Settings.Config.Theme}.xaml");
                var themeUri = new Uri(theme, UriKind.RelativeOrAbsolute);
                if (System.IO.File.Exists(theme))
                {
                    //This is not the best way to do this. We will need to change this
                    Application.Current.Resources.MergedDictionaries.RemoveAt(1);
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = themeUri });
                }
            }
            catch (Exception ex)
            {
                //Default to regular theme
                Logger.Error(ex);
            }

            UpdateLauncherProgressBar(20);

            Logger.Info("Running DB Migrations");
            UpdateLauncherText("Running DB Migrations");
            SQLiteInterface.Init();
            UpdateLauncherProgressBar(30);

            //Set progress before calling for update
            InterfaceHelper.ProgressBarStartValue = 40;

            Logger.Info("Checking for DB Updates");
            UpdateLauncherText("Checking for DB Updates");
            //Check for database update
            await Task.Run(async () =>
            {
                try
                {
                    await CheckForDatabaseUpdateAsync();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            });

            //If we are here then we should be at 100%
            UpdateLauncherProgressBar(100);

            //Reset and load UI assets
            Logger.Info("Loading Assets");
            UpdateLauncherText("Loading Assets");

            //Load all games in whatever the default view is
            await Task.Run(async () =>
            {
                try
                {
                    await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        ModelLoader loader = new ModelLoader();
                        await loader.CreateGamesList(Settings.Config.DefaultPage);
                        ModelData.TotalGames = ModelData.GameCollection.Count;
                        int versions = 0;
                        foreach (var modelData in ModelData.GameCollection)
                        {
                            versions += modelData.Versions.Count;
                        }
                        ModelData.TotalVersions = versions;
                    });
                    //Load the entire GameList before binding it to the view
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            });

            Logger.Info("Launching Atlas");
            UpdateLauncherText("Launching Atlas");
            //System.Threading.Thread.Sleep(1000);
        }

        private async Task CheckForDatabaseUpdateAsync()
        {
            //https://atlas-gamesdb.com/updates/1715651134.update

            string url = "https://atlas-gamesdb.com/api/updates";
            int total = 0;
            int index = 1;
            JArray jsonArray = NetworkHelper.RequestJSON(url);
            if (jsonArray != null)
            {
                //This has to be updated so it will not overwrite previous versions
                int lastDbUpdateVersion = SQLiteInterface.GetLastUpdateVersion();
                total = jsonArray.Count;
                foreach (var jsonitem in jsonArray)
                {
                    //Get data for latest update
                    string date = jsonitem["date"].ToString();
                    string name = jsonitem["name"].ToString();
                    string md5 = jsonitem["md5"].ToString();

                    //Run db check to see if latest update is in database
                    if (Convert.ToInt32(date) > lastDbUpdateVersion || lastDbUpdateVersion == 0)
                    {
                        //Download latest update
                        try
                        {
                            Logger.Info($"Downloading Database Update {index}/{total}");
                            UpdateLauncherText($"Downloading Database Update {index}/{total}");
                            string DownloadUrl = $"https://atlas-gamesdb.com/packages/{name}";
                            string OutputPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data", "updates", name);

                            await NetworkHelper.DownloadFileWithProgress(DownloadUrl, OutputPath);

                            string data = Compression.DecodeLZ4Stream(OutputPath);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateProgressBar.Visibility = System.Windows.Visibility.Visible;
                                UpdateTextBox.Visibility = System.Windows.Visibility.Visible;
                            });

                            Logger.Info($"Processing Update {index}/{total}");
                            UpdateLauncherText($"Processing Update {index}/{total}");
                            await UpdateInterface.ParseUpdate(data);

                            if (UpdateInterface.UpdateCompleted)
                            {
                                SQLiteInterface.InsertUpdateVersion(date, md5);
                            }

                            //update database with data
                            //Database.ProcessUpdate(OutputPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                    index++;
                }
            }
        }

        public void LaunchMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        public void UpdateLauncherText(string text)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                LauncherTextBox.Text = text;
            });
        }
        public void UpdateLauncherProgressBar(int value)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                LauncherProgressBar.Value = value;
            });
        }

    }
}
