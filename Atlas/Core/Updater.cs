using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Atlas.Core
{
    public static class Updater
    {
        private static string AppVersion = string.Empty;
        private static string UpdateDir = string.Empty;
        private static string AtlasDir = string.Empty;
        private static string AtlasExe = string.Empty;

        public static void CheckForUpdates()
        {
            //Set folders
            UpdateDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Atlas");
            AtlasDir = Directory.GetCurrentDirectory();
            AtlasExe = Path.Combine(Directory.GetCurrentDirectory(), "Atlas.exe");

            //Before we do anything, make sure the temp folder is created
            /*if (!Directory.Exists(UpdateDir))
            {
                Directory.CreateDirectory(UpdateDir);
            }
            else //clear all folders in dir
            {
                DirectoryInfo di = new DirectoryInfo(UpdateDir);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }*/
            //Check GH releases for updates and download if found
            string url = "https://api.github.com/repos/towerwatchman/Atlas/releases";
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            //Check if AppVersion has more numebrs
            if(AppVersion.Length > 5)
            {
                AppVersion = AppVersion.Remove(AppVersion.Length - 2); 
            }

            //Request data from Github
            string[] data = RequestJSON(url);
            if (data != null)
            {
                try
                {
                    if (Convert.ToInt32(AppVersion.Replace(".", "")) < Convert.ToInt32(data[0].Replace(".","")))
                    {
                        string message = $"Update available. Would you like to run the update now?\nUpgrade from v{AppVersion} -> {data[2].Split('-')[0]}";
                        string caption = $"Atlas Update";
                        MessageBoxButton buttons = MessageBoxButton.YesNo;

                        // Displays the MessageBox.
                        var result = MessageBox.Show(message, caption, buttons);
                        if (result == MessageBoxResult.Yes)
                        {
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(data[1], Path.Combine(UpdateDir, $"{data[2]}.zip"));
                            }

                            //Check if file downloaded correctly
                            if(File.Exists(Path.Combine(UpdateDir, $"{data[2]}.zip")))
                            {
                                try
                                {
                                    ZipFile.ExtractToDirectory(Path.Combine(UpdateDir, $"{data[2]}.zip"), Path.Combine(UpdateDir, $"{data[2]}"));
                                }
                                catch { }
                                //Check if file directory exist and run powershell
                                if(Directory.Exists(Path.Combine(UpdateDir, $"{data[2]}")))
                                {
                                    string FullUpdateDir = Path.Combine(UpdateDir, data[2], "net8.0-windows\\win-x64");
                                    CopyUpdateFiles(FullUpdateDir, AtlasDir, AtlasExe);
                                }
                            }
                        }
                    }
                }
                catch
                { //Dont do anything };
                }
            }
        }
        public static string[] RequestJSON(string url)
        {
            string response = string.Empty;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            client.DefaultRequestHeaders.Add("Accept" , "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            try
            {
                HttpResponseMessage rspmsg = client.GetAsync(url).Result;
                rspmsg.EnsureSuccessStatusCode();
                response = rspmsg.Content.ReadAsStringAsync().Result;


                if (response != string.Empty)
                {
                    JArray? json = null;

                    //stock
                    JArray jsonArray = JArray.Parse(response);
                    string fullVersion = jsonArray[0]["name"].ToString();
                    string version = jsonArray[0]["name"].ToString().Replace("v", "").Split('-')[0];
                    string download_url = jsonArray[0]["assets"][0]["browser_download_url"].ToString();

                    return new[] { version, download_url, fullVersion }; 
                }
            }
            catch (Exception ex)
            {
                return [];
            }
            return [];
        }

        //Run powershell and moce files to current folder
        public static void CopyUpdateFiles(string UpdateDir, string AtlasDir, string AtlasExe)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $" Start-Sleep -Seconds 1 ;  Copy-item \"{UpdateDir}\\*\" -Destination \"{ AtlasDir }\" -Recurse -force; PAUSE ; start {AtlasExe} ",
                UseShellExecute = false,
                CreateNoWindow = false,                
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }
    }
}


    
