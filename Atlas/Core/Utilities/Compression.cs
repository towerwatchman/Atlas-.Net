using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;

namespace Atlas.Core.Utilities
{
    public static class Compression
    {
        public static void DecodeLZ4Stream(String filename)
        {

            using var source = new MemoryStream(File.ReadAllBytes(filename));
            using var decoder = LZ4Stream.Decode(source);
            using var target = new MemoryStream();
            decoder.CopyTo(target);
            var decoded = target.ToArray();
            var str = System.Text.Encoding.Default.GetString(decoded);

        }
    }
}
