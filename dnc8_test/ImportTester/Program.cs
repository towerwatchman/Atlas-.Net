using System.IO;
using System;
using Microsoft.VisualBasic;

namespace Atlas;

internal class Program
{
    static void Main(string[] args)
    {
        string path = "I:\\Games";
        List<string> root_paths = new List<string>();

        string[] directories = Directory.GetDirectories(path);

        int total_dirs = directories.Length;

        Console.WriteLine($"There are a total of {total_dirs} folders");

        //We need to go through each item and find if it is a folder or file
        foreach (string dir in directories)
        {
           int cur_level = 0;
           int stop_level = 15; //Set to max of 15 levels. There should not be more than 15 at most
           foreach(string t in Directory.GetDirectories(dir,"*", SearchOption.AllDirectories)) 
           {
                cur_level = t.Split('\\').Length;
                string[] s = t.Split('\\');
                if (cur_level <= stop_level)
                {
                    List<string> potential_executables = Executable.DetectExecutable(Directory.GetFiles(t));
                    if (potential_executables.Count > 0)
                    {
                        //Console.WriteLine($"{t} | {cur_level} <= {stop_level}");
                        stop_level = t.Split('\\').Length;
                        Console.WriteLine($"There were {potential_executables.Count} executables found.");
                        foreach(var exe in potential_executables)
                        {
                            Console.WriteLine($"GAME | {exe}");
                        }
                    }
                }

           }

        }


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
