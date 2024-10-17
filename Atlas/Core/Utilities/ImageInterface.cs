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
using ImageMagick;
using System.Collections;

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
                    //
                    

                    var uri = new Uri(path);

                    /*BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = uri;
                    bitmapImage.DecodePixelWidth = (int)width;
                    bitmapImage.CacheOption = BitmapCacheOption.None;
                    bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();*/

                    var img = new MagickImage(path);
                    img.Alpha(AlphaOption.Set);
                    // -virtual-pixel transparent
                    img.VirtualPixelMethod = VirtualPixelMethod.Transparent;

                    // -channel A means that the next operations should only change the alpha channel
                    // - blur 0x8
                    img.Blur(0, 8, Channels.Alpha);
                    // -level 50%,100%
                    img.Level(new Percentage(50), new Percentage(100), Channels.Alpha);

                    BitmapImage bitmapImage;

                    using (MemoryStream ms = new MemoryStream(img.ToByteArray()))
                    {
                        bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = uri;
                        bitmapImage.DecodePixelWidth = (int)width;
                        bitmapImage.CacheOption = BitmapCacheOption.None;
                        bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    }

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
