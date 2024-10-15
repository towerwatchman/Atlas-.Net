using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Atlas.Core.Utilities
{
    public static class ImageInterface
    {
        public static BitmapImage LoadImage(string path, double width, double height)
        {
            var image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/default.jpg"));
            if (path == "")
            {
                return null;
            }
            else
            {
                try
                {
                    //fix so image is smaller.

                    Bitmap original = (Bitmap)System.Drawing.Image.FromFile(path);
                    Bitmap resized;
                    if (original.Width >= original.Height)
                    {
                        //double width = (double)Application.Current.Resources["bannerX"];
                        double width_scaled = (width / original.Width) * original.Height;
                        resized = new Bitmap(original, (int)width, (int)width_scaled);
                    }
                    else
                    {
                        //double height = (double)Application.Current.Resources["bannerY"];
                        double height_scaled = (height / original.Height) * original.Width;
                        resized = new Bitmap(original, (int)height, (int)height_scaled);
                    }


                    //var uri = new Uri(path);
                    var bi = ToBitmapImage(resized);
                    original.Dispose();
                    resized.Dispose();
                    bi.Freeze();
                    return bi;//bi;
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
