using Atlas.UI;
using Atlas.UI.ViewModel;
using K4os.Compression.LZ4.Streams;
using NLog;
using SevenZipExtractor;
using System.Data.OleDb;
using System.IO;
using Windows.UI.Core.Preview;


namespace Atlas.Core.Utilities
{
    public static class Compression
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static string DecodeLZ4Stream(String filename)
        {
            string data = string.Empty;
            try
            {
                using var source = new MemoryStream(File.ReadAllBytes(filename));
                using var decoder = LZ4Stream.Decode(source);
                using var target = new MemoryStream();
                decoder.CopyTo(target);
                var decoded = target.ToArray();
                data = System.Text.Encoding.Default.GetString(decoded);
                //File.WriteAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "test.txt"), data);
            }
            catch (Exception ex) { Logger.Error(ex); }

            return data;
        }

        public static bool ExtractFile(string input, string output, string gameName, int notificationId)
        {
            if (!Directory.Exists(output)) Directory.CreateDirectory(output);

            bool isComplete = false;
            //bool rootFolder = false;
            //string root = "";
            int index = 0;

            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(input))
                {
                    int entries = archiveFile.Entries.Count;


                    /*InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                    {
                        //InterfaceHelper.up
                        InterfaceHelper.GameImportBox.Visibility = System.Windows.Visibility.Visible;
                        InterfaceHelper.GameImportTextBox.Content = $"Extrating Game: {gameName}";
                        InterfaceHelper.GameImportPB.Value = 0;
                        InterfaceHelper.GameImportPB.Maximum = entries;
                        //InterfaceHelper.GameImportPBStatus.Content = $"File: 0\\{entries}";
                    }));*/

                    index = 0;
                    Notifications.UpdateNotification(notificationId, entries, 0, "Starting Extraction");
                    archiveFile.Extract(entry =>
                    {
                        if (entry.FileName != null)
                        {
                            //Span a new task for each file until all are complete
                            Task.Run(() =>
                            {
                                string path =  Path.Combine(output, entry.FileName);
                                ulong filesize = entry.Size;

                                while (true)
                                {
                                    if (Path.HasExtension(path))
                                    {
                                        //If we find a file. Check that it is > 0kb
                                        if (File.Exists(path))
                                        {
                                            FileInfo file = new FileInfo(path);
                                            if ((ulong)file.Length == filesize)
                                            {
                                                InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                                                {
                                                    index++;
                                                    //InterfaceHelper.GameImportPB.Value = index;
                                                    //InterfaceHelper.GameImportPBStatus.Content = $"Extracting File: {Path.GetFileName(path)}";
                                                    Notifications.UpdateNotification(notificationId, entries, index, $"Extracting File: {Path.GetFileName(path)})");
                                                    Notifications.UpdateFileNotification(notificationId, 1,0);
                                                    Logger.Info($"{index}/{entries} - Extracting File: {Path.GetFileName(path)}");
                                                }));
                                                break;
                                            }
                                            
                                            if(file.Length > 0)
                                            {
                                                double pct = (filesize / (ulong)file.Length)*100;
                                                Notifications.UpdateFileNotification(notificationId, (int)pct, 100);

                                                //Notifications.UpdateNotification(notificationId, entries, index, file.Length.ToString());
                                                //Logger.Warn($"{(ulong)file.Length}/{filesize}");
                                            }

                                        }
                                    }
                                    else
                                    {
                                        if (Directory.Exists(path))
                                        {
                                            InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                                            {
                                                index++;
                                                //InterfaceHelper.GameImportPB.Value = index;
                                                //InterfaceHelper.GameImportPBStatus.Content = $"File: {index}\\{entries}";
                                            }));
                                            break;
                                        }
                                    }
                                    System.Threading.Thread.Sleep(50);
                                    if(isComplete)
                                    { break; }
                                }
                               
                            });
                          
                           return Path.Combine(output, entry.FileName); // where to put this particular file
                        }
                        else
                        {
                            return Path.Combine(output, entry.FileName);
                        }     
                    });

                    //If we made it here then we are complete. 
                    InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                    {
                        InterfaceHelper.GameImportPB.Value = entries;
                        InterfaceHelper.GameImportPBStatus.Content = $"File: {entries}\\{entries}";
                    }));
                    //Extraction is complete
                    isComplete = true;
                    Logger.Info("Extraction Complete");
                }
                InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                {
                    InterfaceHelper.GameImportPB.Value = 100;
                    InterfaceHelper.GameImportPBStatus.Content = $"Extraction Complete";
                }));

                //After extracting file, check to see if there was a folder at root.
                if (Directory.GetFiles(output).Count() <= 0)
                {
                    InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                    {
                        //InterfaceHelper.up
                        InterfaceHelper.GameImportTextBox.Content = $"Moving Folder";
                    }));
                    //Get extracted director
                    string extractionDirectory = Directory.GetDirectories(output).First();
                    //move files to correct directory
                    try
                    {
                        MoveDirectory(extractionDirectory, output);
                    }
                    catch(Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    //delete old directory 
                }
            }
            catch (Exception ex)
            {
                isComplete = true; //Make sure to exit out of all loops
                Logger.Error(ex);
                return false;
            }

            return true;
        }

        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile,true);
                }
            }
            Directory.Delete(source,true);
        }

    }
}
