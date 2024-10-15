using Atlas.UI;
using Atlas.Core.Database;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace Atlas.Core
{
    public static class F95Scanner
    {
        public static ObservableCollection<GameDetails> _GameDetailList = new ObservableCollection<GameDetails>();
        public static IEnumerable<GameDetails> GameDetailList { get { return _GameDetailList; } }

        public static bool isRunning = false;
        public static void Start(string path, string format)
        {
            isRunning = true;
            //Set the item list before we do anything else

            List<string> root_paths = new List<string>();

            string[] directories = Directory.GetDirectories(path);

            int total_dirs = directories.Length;

            //Set min and max for progress bar
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                InterfaceHelper.GameScannerProgressBar.Value = 0;
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
                int folder_size = 0;
                int cur_level = 0;
                int stop_level = 15; //Set to max of 15 levels. if an executabl is found this will update. 

                //Update Progressbar
                ittr++;
                UpdateProgressBar(ittr);
                try
                {
                    foreach (string t in Directory.GetDirectories(dir, "*", SearchOption.AllDirectories))
                    {
                        
                        //Check if current folder has files, If not, skip
                        if (Directory.GetFiles(t).Count() > 0)
                        {
                            cur_level =  t.Split('\\').Length;
                            string[] s = t.Split('\\');
                            if (cur_level <= stop_level)
                            {
                                Logging.Logger.Warn(t);
                                List<string> potential_executables = Executable.DetectExecutable(Directory.GetFiles(t));
                                if (potential_executables.Count > 0)
                                {
                                    //check to see how to parse data

                                    stop_level = t.Split('\\').Length;
                                    //Now that we have a list of executables, we need to try and parse the engine, version, name etc..,
                                
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
            //Try to bind item source 
            UpdateBannerView();
            //outputFile.Close();
        }
        private static void AddGameToBannerView(GameDetails gd)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.Datagrid.Items.Add(gd);
            }));
        }
        private static void UpdateBannerView()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.Datagrid.Items.Clear();
                InterfaceHelper.Datagrid.ItemsSource = _GameDetailList;
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
