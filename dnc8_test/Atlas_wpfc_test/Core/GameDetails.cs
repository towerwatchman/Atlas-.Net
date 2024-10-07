using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Atlas.UI;

namespace Atlas.Core;

public static class Executable
{
    static bool skip_x86 = true;
    static string[] blacklist = [
        "UnityCrashHandler64.exe",
        "UnityCrashHandler32.exe",
        "payload.exe",
        "nwjc.exe",
        "notification_helper.exe",
        "nacl64.exe",
        "chromedriver.exe",
        "Squirrel.exe",
        "zsync.exe",
        "zsyncmake.exe",
        "cmake.exe",
        "pythonw.exe",
        "python.exe",
        "dxwebsetup.exe",
        "README.html",
        "manual.htm",
        "unins000.exe",
        "UE4PrereqSetup_X64.exe",
        "UEPrereqSetup_x64.exe",
        "credits.html",
        "LICENSES.chromium.html",
        "Uninstall.exe",
        "CONFIG_dl.exe"];
    static string[] lin_exe = [".sh"];
    static string[] win_exe = [".exe"];
    static string[] osx_exe = [".dmg"];
    static string[] oth_exe = [".swf", ".flv", ".f4v", ".rag", ".cmd", ".bat", ".jar", ".html"];

    public static List<string> DetectExecutable(string[] files) 
    {
        string[] extensions = GetExtensions();
        List<string> potential_executables = []; // This has to reset every time we change folders   
        foreach(var file in files)
        {
            if (File.Exists(file))
            {
                string filename = Path.GetFileName(file);
                foreach (var extension in extensions)
                {
                    if (filename.ToLower().Trim().Contains(extension.ToLower().Trim()))
                    {
                        //check if file is in blacklist and skip
                        if (!blacklist.Any(x => x == filename))// .Any(s => s.Equals(filename)));
                        {
                            //check if we skip 32bit version of files
                            if (skip_x86 && filename.Contains("-32")) break;

                            potential_executables.Add(Path.GetFileName(file));
                            break;
                        }
                    }
                }
            }
        }
        //!!!NOTE Now that we have all executables, check if there is an exe. If there is, remove everything else !!!
        // NEEDS TO BE TESTED
        if (potential_executables.Count > 0)
        {                
            if(potential_executables.Any(s => s.EndsWith(".exe")))
            {
                potential_executables.RemoveAll(x => !x.Contains(".exe"));
            }
            return potential_executables;   
        }
        else
        {
            return [];
        }
    }

    static string[] GetExtensions()
    {
        //combine all extensions
        //if settings.os == "Windows":
        return [.. win_exe, .. oth_exe];
        //if (settings.os == "Linux"):
        //return executable.lin_exe + executable.oth_exe
    }
}

public static class Engine
{
    //implement later
    public static string FindEngine(string[] file_list)
    {
        string engine = "Others";

        return engine;
    }
}

public static class Details
{   
    public static string[] ParseDetails(string s)
    {
        string[] data = [];
        string[] text = s.Split('\\');

        if (text.Length == 1)
        {
            data = ParseSingleFileName(text[0]);
        }
        if(text.Length == 2)
        {
            int distance = CheckForSimilarStrings(text[0], text[1]);

            if (distance <20)
            {
                data = ParseSingleFileName(text[1]);
            }
            else
            {
                //Check to see if the first folder has any numbers in it
                //Assume Title, Version
                data = [text[0], text[1]];
            }
        }
        if (text.Length > 2)
        {
            data = [text[1], text[2], text[0]];
        }

        return data;
    }

    private static string[] ParseSingleFileName(string v)
    {
        string[] file_data = [];
        
        if(v.Contains('-'))
        {
            file_data = ParseStringByDelimeter(v, '-');
        }
        else
        {
            if(v.Contains('_'))
            {
                file_data = ParseStringByDelimeter(v, '_');
            }
            else
            {
                if(v.Contains(' '))
                {
                    file_data = ParseStringByDelimeter(v, ' ');
                }
                else
                {
                    file_data = [AddSpaces(v), ""];
                }
            }
        }

        return file_data; 
    }

    private static string[] ParseStringByDelimeter(string v, char c)
    {
        string title = string.Empty;
        string version = string.Empty;
        string[] slist = v.Split(c);
        bool is_version = false;
        for (int i = 0; i < slist.Length; i++)
        {
            var item = slist[i];   
            if(item.Contains(' '))
            {
                var temp = ParseSingleFileName(item);
                title = temp[0];
                version = temp[1];
            }
            // Assume first item will always be a part of the title.
            //Check if it has a number or has an OS name
            else
            {
                if (!CheckOSNames(item) && !CheckLanguages(item))
                {
                    if(!(i > 0  && IsDigit(item)) && is_version == false)
                    {
                        var version_result = FindVersionType(item);
                        title += version_result[0];
                        version += version_result[1];
                        //Check for version chapter or season
                    }
                    else
                    {
                        is_version = true;
                        version += item;
                    } 
                }
            }
        }
        return [AddSpaces(title.Replace("_","")), version];
    }


    private static bool CheckOSNames(string v)
    {
        string[] os = { "pc", "win", "linux", "windows", "unc", "win64" };
        if (os.Any(x => v.ToLower().Contains(x)))
        {
            return true;
        }
        return false;
    }

    private static bool CheckLanguages(string v)
    {
        string[] languages = { "japanses", "english" };
        if(languages.Any(x=> v.Contains(x))) 
        {
            return true;
        }
        return false;
    }
    private static bool IsDigit(string v)
    {
        foreach( char c in v )
        {
            if(char.IsDigit(c)) return true;
        } 
        return false;
    }
    private static string AddSpaces(string v)
    {
        return Regex.Replace(v, "[A-Z]", @" $0");
    }

    private static string[] FindVersionType(string v)
    {
        string title = string.Empty;
        string version = string.Empty;
        string[] delimiters = { "final", "episode", "chapter", "version", "season", "v." };

        if(delimiters.Any(x=> v.Contains(x)))
        {
            return ["", v];
        }
        else
        {
            return [v, ""];
        }
    }

    private static int CheckForSimilarStrings(string input1, string input2)
    {
        if (string.IsNullOrEmpty(input1))
            return string.IsNullOrEmpty(input2) ? 0 : input2.Length;

        if (string.IsNullOrEmpty(input2))
            return string.IsNullOrEmpty(input1) ? 0 : input1.Length;

        int input1Length = input1.Length;
        int input2Length = input2.Length;

        int[,] distance = new int[input1Length + 1, input2Length + 1];

        for (int i = 0; i <= input1Length; distance[i, 0] = i++) ;
        for (int j = 0; j <= input2Length; distance[0, j] = j++) ;

        for (int i = 1; i <= input1Length; i++)
        {
            for (int j = 1; j <= input2Length; j++)
            {
                int cost = (input2[j - 1] == input1[i - 1]) ? 0 : 1;

                distance[i, j] = Math.Min(
                                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[input1Length, input2Length];
    }
}
