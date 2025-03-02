﻿using Atlas.Core.Database;
using Atlas.UI;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Formats.Tar;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Shapes;
using Windows.Devices.Printers;

namespace Atlas.Core
{
    public static class F95Scanner
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static ObservableCollection<GameDetails> _GameDetailList;
        public static IEnumerable<GameDetails> GameDetailList { get { return _GameDetailList; } }

        public static string game_path = "";
        public static string title = "";
        public static string version = "";
        public static string creator = "";
        public static string game_engine = "Others";
        public static int potentialGames = 0;

        public static bool isRunning = false;
        public static Task Start(string path, string format, string[] extensions)
        {
            potentialGames = 0;
            _GameDetailList = new ObservableCollection<GameDetails>();
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
                InterfaceHelper.ImporterScanTextBox.Text = $"0/{total_dirs} Folders Scanned";
            }));


            Logger.Info($"There are a total of {total_dirs} folders\n");
            //StreamWriter outputFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Atlas_Output.txt"));
            //We need to go through each item and find if it is a folder or file

            int ittr = 0;
            
            foreach (string dir in directories)
            {
                game_path = string.Empty;
                title = string.Empty;
                version = string.Empty;
                creator = string.Empty;
                game_engine = "Others";
                bool found_executable = false;
                Logger.Info(dir);
                
                //int folder_size = 0;
                int cur_level = 0;
                int stop_level = 15; //Set to max of 15 levels. There should not be more than 15 at most

                //Update Progressbar
                ittr++;
                UpdateProgressBar(ittr, total_dirs);

                try
                {
                    foreach (string t in Directory.GetDirectories(dir, "*", SearchOption.AllDirectories))
                    {
                        cur_level = t.Split('\\').Length;
                        string[] s = t.Split('\\');
                        if (cur_level <= stop_level)
                        {                             
                            if(!found_executable)
                            {
                                found_executable = FindGame(t, format, extensions, path, stop_level, potentialGames);
                                if(found_executable)
                                {
                                    stop_level = cur_level;
                                }
                            }
                            //conintue checking folders for other versions using the same stop level
                            else
                            {
                                FindGame(t, format, extensions, path, stop_level, potentialGames);
                            }                            
                        }
                    }

                    /*if(Directory.GetDirectories(dir).Length <= 0) 
                    {
                        foreach (var file in Directory.GetFiles(dir))
                        {
                            FindGame(file, format, extensions, path, stop_level, potentialGames, true);
                        }
                    }*/

                    //if we cant find any folders, the check for files
                    if (!found_executable)
                    {
                        foreach (string f in Directory.GetFiles(dir))
                        {
                            FindGame(f, format, extensions, path, stop_level, potentialGames, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex);
                }
            }

            //if there are no folders, then check for files. 
            //Try to bind item source 
            UpdateBannerView();
            //outputFile.Close();

            return Task.CompletedTask;
        }

        public static bool FindGame(string t, string format, string[] extensions, string path, int stop_level, int potentialGames, bool isFile = false)
        {
  
            List<string> potential_executables = new List<string>();
            if(isFile)
            {
                potential_executables = Executable.DetectExecutable([t], extensions);
            }
            else
            {
                potential_executables = Executable.DetectExecutable(Directory.GetFiles(t), extensions);
            }
                
                
            if (potential_executables.Count > 0)
            {
                //check to see how to parse data

                stop_level = t.Split('\\').Length;
                //Now that we have a list of executables, we need to try and parse the engine, version, name etc..,
                game_path = t;
                string[] file_list = Walk(t);//This is the list we will use to determine the engine
                game_engine = GameEngine.FindEngine(file_list);
                string[] game_data = Details.ParseDetails(t.Replace($"{path}\\", "").Replace('-','_'));
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
                        title = strArr[title_index].Trim();
                    }
                    if (creator_index > -1)
                    {
                        creator = strArr[creator_index].Trim();
                    }
                    if (engine_index > -1)
                    {
                        game_engine = strArr[engine_index].Trim();
                    }
                    if (version_index > -1)
                    {
                        version = strArr[version_index].Trim();
                    }
                }


                //Check the database to see if we have a match
                List<string[]> data = new List<string[]>();
                
                     data = SQLiteInterface.GetAtlasId(title, creator);


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
                string f95_id = "";
                if (data.Count == 0)
                {
                    ResultVisibilityState = Visibility.Hidden;
                }
                else if (data.Count > 0)
                {
                    Id = data[0][0];
                    title = data[0][1];
                    creator = data[0][2];
                    game_engine = data[0][3];
                    f95_id = SQLiteInterface.FindF95ID(Id);
                    ResultVisibilityState = Visibility.Hidden;
                }
               
                if (data.Count > 1)
                {
                    foreach (var item in data)
                    {
                        results.Add($"{item[0]} | {item[1]} | {item[2]}");
                    }
                    ResultVisibilityState = Visibility.Visible;
                }
               

                //Make sure version does not include the extension
                foreach(var ext in extensions)
                {
                    if(version.Contains(ext))
                    {
                        version = version.Replace(ext, "");
                    }
                }

                //CHECK IF DATA IS ALREADY IN THE DATBASE. IF IT IS THEN UPDATE THE TABLE
                bool record_exist = SQLiteInterface.CheckIfRecordExist(title, creator, version);
                //if no record found, check against game path
                if (!record_exist)
                {
                    record_exist = SQLiteInterface.CheckIfPathExist(t, title);
                }

                var gd = new GameDetails
                {
                    Id = Id,
                    f95_Id = f95_id,
                    Title = title.Trim().Replace(" ", ""),
                    Version = version.Trim().Replace(" ", ""),
                    Creator = creator.Trim().Replace(" ", ""),
                    Engine = game_engine.Trim().Replace(" ", ""),
                    SingleEngineVisible = SingleEngineVisible,
                    Executable = [.. potential_executables],
                    SingleExecutable = SingleExecutable,
                    MultipleEngineVisible = MultipleEngineVisible,
                    Folder = t,
                    Results = results,
                    RecordExist= record_exist,
                    ResultVisibilityState = ResultVisibilityState
                };

                if (title != "")
                {
                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (!_GameDetailList.Where(x => x.Folder == t).Any())
                            {
                                if (!record_exist)
                                {
                                    _GameDetailList.Add(gd);
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex);
                    }
                    Task.Run(() =>
                    {
                        try
                        {
                            if (!record_exist)
                            {
                                AddGameToBannerView(gd);

                                Console.Out.WriteLine(_GameDetailList.Count.ToString());
                                potentialGames = _GameDetailList.Count;
                                UpdatePotentialGames(potentialGames);
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                        //UpdateBannerView();
                    });
                }
                return true;
            }
            return false;
        }

        private static void AddGameToBannerView(GameDetails gd)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.Datagrid.Items.Add(gd);
                InterfaceHelper.Datagrid.IsReadOnly = true;
            }));
        }
        private static void UpdateBannerView()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                InterfaceHelper.Datagrid.Items.Clear();
                InterfaceHelper.Datagrid.ItemsSource = _GameDetailList;
                InterfaceHelper.Datagrid.Items.Refresh();
                InterfaceHelper.Datagrid.IsReadOnly = false;
            }));
        }
        public static string[] Walk(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            string[] list = null;
            if (attr.HasFlag(FileAttributes.Directory))
            {

                string[] directories = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                list = directories.Concat(files).ToArray();
            }
            else
            {
                list = [path];
            }

            return list;
        }
        private static void UpdateProgressBar(int value, int total_dirs)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                InterfaceHelper.GameScannerProgressBar.Value = value;
                InterfaceHelper.ImporterScanTextBox.Text = $"{value}/{total_dirs} Folders Scanned";
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
