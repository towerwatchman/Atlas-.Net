using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Atlas.Core.Utilities
{
    public static class ImageInterface
    {
        public static BitmapImage LoadImage(string path, double width, double height)
        {
            var image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/default.jpg"));
            if (path == "")
            {
                return image;
            }
            else
            {
                try
                {
                    var uri = new Uri(path);

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = uri;
                    bitmapImage.DecodePixelWidth = (int)width;
                    bitmapImage.CacheOption = BitmapCacheOption.None;
                    bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;//bi;
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return image;
                }
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
