using K4os.Compression.LZ4.Streams;
using NLog;
using System.IO;
using SevenZipExtractor;


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

        public static bool ExtractFile(string input, string output)
        {
            if(!Directory.Exists(output)) Directory.CreateDirectory(output);
            bool rootFolder = false;
            string root = "";
            int index = 0;

            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(input))
                {
                    foreach (Entry entry in archiveFile.Entries)
                    {
                        if (entry.FileName.Split('\\').Length > 1)
                        {
                            rootFolder = true;
                            root = entry.FileName.Split('\\')[0];
                        }

                        string obj = entry.FileName.Replace(root, "");
                        Console.WriteLine(obj);

                        // extract to file
                        string path = $"{output}{obj}";
                        entry.Extract(path);
                    }
                }
            }
            catch { return false; }

            return true;
        }
    }
}
