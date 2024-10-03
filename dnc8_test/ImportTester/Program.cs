using System.IO;
using System;
using Microsoft.VisualBasic;
using static System.Net.WebRequestMethods;
using System.Data;

namespace Atlas;

internal class Program
{
    static void Main(string[] args)
    {
        string path = "I:\\_Extracted";
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
            
           foreach (string t in Directory.GetDirectories(dir,"*", SearchOption.AllDirectories)) 
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
                        string[] game_data = Details.ParseDetails(t.Replace($"{path}\\",""));
                        if(game_data.Length > 0)
                        {
                            title = game_data[0];
                            version = game_data[1];
                        }
                        if(game_data.Length > 2)
                        {
                            creator = game_data[2];
                        }

                        string output = $"Title: {title}\nCreator: {creator}\nEngine: {game_engine}\nVersion: {version}\n" ;
                        foreach(var exe in potential_executables)
                        {
                            output += $" + Potential Executable: {Path.GetFileName(exe)}\n";
                        }
                       output += ($"Folder: {t}\nFolder Size: {folder_size}\n");
                       output += ("*-----------------------------------------------------*");
                       Console.WriteLine(output);
                        outputFile.WriteLine(output);
                    }
                }

           }
        }
        outputFile.Close();

        //root_paths.Add(Path.GetPathRoot(Item));
        //Console.WriteLine(Item);


        Console.ReadLine();
    }

    public static string[] Walk(string path)
    {
        string[] directories = Directory.GetDirectories(path);
        string[] files = Directory.GetFiles(path);
        var list = directories.Concat(files).ToArray();

        return list;
    }
}
