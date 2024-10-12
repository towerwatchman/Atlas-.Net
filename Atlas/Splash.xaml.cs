using Atlas.Core;
using Atlas.Core.Database;
using Atlas.Core.Network;
using Atlas.Core.Utilities;
using Atlas.UI;
using Config.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;
using static System.Net.WebRequestMethods;

namespace Atlas
{
    /// <summary>
    /// Check for updates
    /// Set Folders
    /// Add Settings
    /// Run db migrations
    /// </summary>
    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();

            //assign progressbar to helper
            InterfaceHelper.SplashWindow = this;
            InterfaceHelper.SplashProgressBar = pbSplash;
            InterfaceHelper.SplashTextBox = tbSplash;

            //Check if program is already open
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1) System.Diagnostics.Process.GetCurrentProcess().Kill();
            //Discard await warning
            Task.Run(async () => {
                try
                {
                    await Init();
                    //Launch Main Window
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        LaunchMainWindow();
                    }));
                }
                catch(Exception ex)
                {
                    Logging.Logger.Error(ex);
                }
            });

        }

        public async Task Init()
        {
            UpdateSplashText( "Check For Updates");
            //Check for updates
            await Task.Run(() =>
            {
                try
                {
                    Updater.CheckForUpdates();
                }
                catch (Exception ex)
                {
                    Logging.Logger.Error(ex);
                }
            });
            UpdateSplashProgressBar(10);

            UpdateSplashText("Updating Folders");
            //Set folders
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "games"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "images"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "logs"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "updates"));
            UpdateSplashProgressBar(20);

            UpdateSplashText("Updating xaml Dependencies");
            //Add Settings
            Settings.Config = new ConfigurationBuilder<SettingInterface>().UseIniFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.ini")).Build();

            //Set the default theme file
            string theme = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "themes", Settings.Config.Theme);
            var themeUri = new Uri(theme, UriKind.RelativeOrAbsolute);
            try
            {
                if (System.IO.File.Exists(theme))
                {
                    Application.Current.Resources.MergedDictionaries.RemoveAt(2);
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = themeUri });
                }
            }
            catch (Exception ex)
            {
                //Default to regular theme
                Logging.Logger.Error(ex);
            }
            UpdateSplashProgressBar(30);

            UpdateSplashText("Running DB Migrations");
            Database.Init();
            UpdateSplashProgressBar(40);

            UpdateSplashText("Checking for DB Updates");
            //Check for database update
            await Task.Run(async () =>
            {
                await CheckForDatabaseUpdateAsync();
            });

        }

        private async Task CheckForDatabaseUpdateAsync()
        {
            //https://atlas-gamesdb.com/updates/1715651134.update

            string url = "https://atlas-gamesdb.com/api/updates";
            JArray jsonArray = NetworkInterface.RequestJSON(url);
            if (jsonArray != null)
            {
                //Get data for latest update
                string date = jsonArray[0]["date"].ToString();
                string name = jsonArray[0]["name"].ToString();
                string md5 = jsonArray[0]["md5"].ToString();

                //Run db check to see if latest update is in database

                //Download latest update
                try 
                {
                    string DownloadUrl = $"https://atlas-gamesdb.com/packages/{name}";
                    string OutputPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "updates",name);
                    NetworkInterface networkInterface = new NetworkInterface();
                    await networkInterface.DownloadFile(DownloadUrl, OutputPath);

                    string data = Compression.DecodeLZ4Stream(OutputPath);
                    UpdateInterface.ParseUpdate(data);

                    //update database with data
                    //Database.ProcessUpdate(OutputPath);
                }
                catch(Exception ex)
                {
                    Logging.Logger.Error(ex);
                }
            }
        }

        public void LaunchMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        public void UpdateSplashText(string text)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
                tbSplash.Text = text;
            });
        }
        public void UpdateSplashProgressBar(int value)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, () =>
            {
               pbSplash.Value = value;
            });
        }
    }
}
