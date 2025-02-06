using K4os.Compression.LZ4.Streams;
using NLog;
using System.IO;
using SevenZipExtractor;
using Atlas.UI;
using Windows.Media.AppBroadcasting;


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
                //File.WriteAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "test.txt"), data);
            }
            catch (Exception ex) { Logger.Error(ex); }

            return data;
        }

        public static bool ExtractFile(string input, string output, string gameName)
        {
            if(!Directory.Exists(output)) Directory.CreateDirectory(output);
            bool rootFolder = false;
            string root = "";
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
                    foreach (Entry entry in archiveFile.Entries)
                    {
                        index++;
                        if (entry.FileName.Split('\\').Length > 1)
                        {
                            rootFolder = true;
                            root = entry.FileName.Split('\\')[0];
                        }

                        string obj = root != "" ? entry.FileName.Replace(root, "") : entry.FileName;
                        if(!obj.Contains('\\'))
                        {
                            obj = '\\' + obj;
                        }
                        //Console.WriteLine(obj);

                        // extract to file
                        string path = $"{output}{obj}";

                        entry.Extract(path);
                        InterfaceHelper.LauncherWindow.Dispatcher.Invoke((Action)(() =>
                        {
                            InterfaceHelper.GameImportPB.Value = index;
                            InterfaceHelper.GameImportPBStatus.Content = $"File: {index}\\{entries}";
                        }));
                    }
                }
            }
            catch(Exception ex) {
                Logger.Error(ex);
                return false; 
            }

            return true;
        }
    }
}
