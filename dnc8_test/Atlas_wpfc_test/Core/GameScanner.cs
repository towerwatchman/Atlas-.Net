﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Atlas.UI;

namespace Atlas.Core
{
    public static class GameScanner
    {
        private static List<GameDetails> GamesList = new List<GameDetails>();

        public struct GameDetails
        {
            public string Title { set; get; }
            public string Creator { set; get; }
            public string Engine { set; get; }
            public string Version { set; get; }
            public List<string> Executable { set; get; }
            public string Folder { set; get; }
        }

        public static void Start(string path)
        {

            List<string> root_paths = new List<string>();

            string[] directories = Directory.GetDirectories(path);

            int total_dirs = directories.Length;
            string root_path = "";
            string game_path = "";
            string title = "";
            string version = "";
            string creator = "";
            int folder_size = 0;

            Console.WriteLine($"There are a total of {total_dirs} folders\n");
            StreamWriter outputFile = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "Atlas_Output.txt"));
            //We need to go through each item and find if it is a folder or file
            foreach (string dir in directories)
            {
                int cur_level = 0;
                int stop_level = 15; //Set to max of 15 levels. There should not be more than 15 at most

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

                            var gd = new GameDetails { Title = title, Version = version, Creator = creator, Engine = game_engine, Executable = potential_executables.ToList(), Folder = t  };

                            GamesList.Add(gd);
                            UpdateBannerView();
                        }
                    }
                }
            }
            outputFile.Close();
        }

        private static void UpdateBannerView()
        {
            if (WpfHelper.Datagrid.Items.Count <= 0)
            {
                WpfHelper.Datagrid.ItemsSource = GamesList;
            }
        }

        public static string[] Walk(string path)
        {
            string[] directories = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
            var list = directories.Concat(files).ToArray();

            return list;
        }
    }
}
