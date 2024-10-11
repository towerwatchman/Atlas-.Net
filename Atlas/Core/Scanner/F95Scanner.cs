using Atlas.UI;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Atlas.Core
{
    public static class F95Scanner
    {
        public static ObservableCollection<GameDetails> _GameDetailList = new ObservableCollection<GameDetails>();
        public static IEnumerable<GameDetails> GameDetailList { get { return _GameDetailList; } }

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
            StreamWriter outputFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Atlas_Output.txt"));
            //We need to go through each item and find if it is a folder or file

            int ittr = 0;
            int potentialGames = 0;
            foreach (string dir in directories)
            {
                string game_path = "";
                string title = "";
                string version = "";
                string creator = "";
                int folder_size = 0;
                int cur_level = 0;
                int stop_level = 15; //Set to max of 15 levels. There should not be more than 15 at most

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
                                stop_level = t.Split('\\').Length;
                                //Now that we have a list of executables, we need to try and parse the engine, version, name etc..,
                                game_path = t;
                                string[] file_list = Walk(t);//This is the list we will use to determine the engine
                                string game_engine = Engine.FindEngine(file_list);
                                string[] game_data = Details.ParseDetails(t.Replace($"{path}\\", ""));
                                if (game_data.Length > 0)
                                {
                                    title = game_data[0];
                                    version = game_data[1];
                                }
                                if (game_data.Length > 2)
                                {
                                    creator = game_data[2];
                                }

                                string output = $"Title: {title}\nCreator: {creator}\nEngine: {game_engine}\nVersion: {version}\n";
                                foreach (var exe in potential_executables)
                                {
                                    output += $" + Potential Executable: {Path.GetFileName(exe)}\n";
                                }
                                output += ($"Folder: {t}\nFolder Size: {folder_size}\n");
                                output += ("*-----------------------------------------------------*");
                                Console.WriteLine(output);
                                outputFile.WriteLine(output);

                                var gd = new GameDetails
                                {
                                    Title = title.Trim(),
                                    Version = version.Trim(),
                                    Creator = creator.Trim(),
                                    Engine = game_engine.Trim(),
                                    Executable = potential_executables.ToList(),
                                    Folder = t
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
                                        UpdateBannerView();
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Logger.Warn(ex);
                }
            }
            outputFile.Close();
        }

        private static void UpdateBannerView()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.Datagrid.Items.Refresh();
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
