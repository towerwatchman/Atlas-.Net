using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas
{
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

                                potential_executables.Add(file);
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
}
