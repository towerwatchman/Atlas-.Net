using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using NLog;

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
            catch(Exception ex) { Logger.Error(ex); }

            return data;
        }
    }
}
