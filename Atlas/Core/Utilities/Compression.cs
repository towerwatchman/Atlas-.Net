﻿using Atlas.UI;
using K4os.Compression.LZ4.Streams;
using NLog;
using SevenZipExtractor;
using System.Data.OleDb;
using System.IO;


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

        public static bool ExtractFile(string input, string output, string gameName)
        {
            if (!Directory.Exists(output)) Directory.CreateDirectory(output);
            //bool rootFolder = false;
            //string root = "";
            int index = 0;

            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(input))
                {
                    int entries = archiveFile.Entries.Count;
                    InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                    {
                        //InterfaceHelper.up
                        InterfaceHelper.GameImportBox.Visibility = System.Windows.Visibility.Visible;
                        InterfaceHelper.GameImportTextBox.Content = $"Extrating Game: {gameName}";
                        InterfaceHelper.GameImportPB.Value = 0;
                        InterfaceHelper.GameImportPB.Maximum = entries;
                        InterfaceHelper.GameImportPBStatus.Content = $"File: 0\\{entries}";
                    }));

                    index = 0;
                    archiveFile.Extract(entry =>
                    {
                        if (entry.FileName != null)
                        {
                            index++;
                            InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                            {
                                InterfaceHelper.GameImportPB.Value = index;
                                InterfaceHelper.GameImportPBStatus.Content = $"File: {index}\\{entries}";
                            }));
                            return Path.Combine(output, entry.FileName); // where to put this particular file
                        }
                        else
                        {
                            return Path.Combine(output, entry.FileName); // where to put this particular file
                        }

                    });                   
                }
                //After extracting file, check to see if there was a folder at root.
                if(Directory.GetFiles(output).Count() <= 0)
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
