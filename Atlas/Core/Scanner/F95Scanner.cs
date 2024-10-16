using Atlas.UI;
using Atlas.Core.Database;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using NLog;

namespace Atlas.Core
{
    public static class F95Scanner
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static ObservableCollection<GameDetails> _GameDetailList = new ObservableCollection<GameDetails>();
        public static IEnumerable<GameDetails> GameDetailList { get { return _GameDetailList; } }

        public static bool isRunning = false;
        public static void Start(string path, string format)
        {
            //Set the item list before we do anything else

            List<string> root_paths = new List<string>();

            string[] directories = Directory.GetDirectories(path);

            int total_dirs = directories.Length;

            //Set min and max for progress bar
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                InterfaceHelper.GameScannerProgressBar.Visibility = Visibility.Visible;
                InterfaceHelper.GameScannerProgressBar.Minimum = 0;
                InterfaceHelper.GameScannerProgressBar.Maximum = total_dirs;
            }));


            Console.WriteLine($"There are a total of {total_dirs} folders\n");
            //StreamWriter outputFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Atlas_Output.txt"));
            //We need to go through each item and find if it is a folder or file

            int ittr = 0;
            int potentialGames = 0;
            foreach (string dir in directories)
            {
                string game_path = "";
                string title = "";
                string version = "";
                string creator = "";
                string game_engine = "";
                //int folder_size = 0;
                int cur_level = 0;
                int stop_level = 6; //Set to max of 15 levels. There should not be more than 15 at most

                //Update Progressbar
                ittr++;
                UpdateProgressBar(ittr);
                try
                {
                    foreach (string t in Directory.GetDirectories(dir, "*", SearchOption.AllDirectories))
                    {
                        cur_level = t.Split('\\').Length;
                        string[] s = t.Split('\\');
                        if (cur_level <= stop_level)
                        {

                            List<string> potential_executables = Executable.DetectExecutable(Directory.GetFiles(t));
                            if (potential_executables.Count > 0)
                            {
                                //check to see how to parse data

                                stop_level = t.Split('\\').Length;
                                //Now that we have a list of executables, we need to try and parse the engine, version, name etc..,
                                game_path = t;
                                string[] file_list = Walk(t);//This is the list we will use to determine the engine
                                game_engine = Engine.FindEngine(file_list);
                                string[] game_data = Details.ParseDetails(t.Replace($"{path}\\", ""));
                                if (format == "")
                                {

                                    if (game_data.Length > 0)
                                    {
                                        title = game_data[0];
                                        version = game_data[1];
                                    }
                                    if (game_data.Length > 2)
                                    {
                                        creator = game_data[2];
                                    }
                                }
                                else
                                {
                                    //try to parse based on pattern matching
                                    string[] parseFormat = format.ToUpper().Replace("{", "").Replace("}", "").Split("\\");
                                    string[] strArr = t.Replace($"{path}\\", "").Split("\\");

                                    var title_index = Array.FindIndex(parseFormat, row => row == "TITLE");
                                    var creator_index = Array.FindIndex(parseFormat, row => row == "CREATOR");
                                    var engine_index = Array.FindIndex(parseFormat, row => row == "ENGINE");
                                    var version_index = Array.FindIndex(parseFormat, row => row == "VERSION");

                                    if (title_index > -1)
                                    {
                                        title = strArr[title_index];
                                    }
                                    if (creator_index > -1)
                                    {
                                        creator = strArr[creator_index];
                                    }
                                    if (engine_index > -1)
                                    {
                                        game_engine = strArr[engine_index];
                                    }
                                    if (version_index > -1)
                                    {
                                        version = strArr[version_index];
                                    }
                                }


                                //Check the database to see if we have a match
                                List<string[]> data = SQLiteInterface.GetAtlasId(title, creator);

                                List<string> results = new List<string>();
                                string SingleExecutable = string.Empty;
                                Visibility ResultVisibilityState = Visibility.Visible;
                                Visibility SingleEngineVisible = Visibility.Visible;
                                Visibility MultipleEngineVisible = Visibility.Visible;

                                if (potential_executables.Count == 1)
                                {
                                    SingleEngineVisible = Visibility.Visible;
                                    MultipleEngineVisible = Visibility.Collapsed;
                                    SingleExecutable = potential_executables[0].Trim();
                                }

                                string Id = "";
                                if (data.Count == 0)
                                {
                                    ResultVisibilityState = Visibility.Hidden;
                                }
                                else if (data.Count == 1)
                                {
                                    Id = data[0][0];
                                    title = data[0][1];
                                    creator = data[0][2];
                                    game_engine = data[0][3];
                                    ResultVisibilityState = Visibility.Hidden;
                                }
                                else
                                {
                                    if (data.Count > 1)
                                    {
                                        foreach (var item in data)
                                        {
                                            results.Add($"{item[0]} | {item[1]} | {item[2]}");
                                        }
                                    }
                                }

                                var gd = new GameDetails
                                {
                                    Id = Id,
                                    Title = title.Trim(),
                                    Version = version.Trim(),
                                    Creator = creator.Trim(),
                                    Engine = game_engine.Trim(),
                                    SingleEngineVisible = SingleEngineVisible,
                                    Executable = [.. potential_executables],
                                    SingleExecutable = SingleExecutable,
                                    MultipleEngineVisible = MultipleEngineVisible,
                                    Folder = t,
                                    Results = results,
                                    ResultVisibilityState = ResultVisibilityState
                                };

                                if (title != "")
                                {
                                    Application.Current.Dispatcher.BeginInvoke(() =>
                                    {
                                        _GameDetailList.Add(gd);
                                        potentialGames++;
                                        UpdatePotentialGames(potentialGames);
                                        //dg.Items.Refresh();
                                    });
                                    Task.Run(() =>
                                    {
                                        AddGameToBannerView(gd);
                                        //UpdateBannerView();
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            }
            //Try to bind item source 
            UpdateBannerView();
            //outputFile.Close();
        }
        private static void AddGameToBannerView(GameDetails gd)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.Datagrid.Items.Add(gd);
                InterfaceHelper.Datagrid.IsEnabled = false;
            }));
        }
        private static void UpdateBannerView()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.Datagrid.Items.Clear();
                InterfaceHelper.Datagrid.ItemsSource = _GameDetailList;
                InterfaceHelper.Datagrid.Items.Refresh();
                InterfaceHelper.Datagrid.IsEnabled = true;
            }));
        }
        public static string[] Walk(string path)
        {
            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            var list = directories.Concat(files).ToArray();

            return list;
        }
        private static void UpdateProgressBar(int value)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {                
                InterfaceHelper.GameScannerProgressBar.Value = value;
            }));
        }
        private static void UpdatePotentialGames(int value)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                InterfaceHelper.PotentialGamesTextBox.Text = $"Found {value.ToString()} Games";
            }));
        }
    }
}
