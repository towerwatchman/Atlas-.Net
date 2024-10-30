using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using System.Collections;
using System.Windows.Media;
using NLog;
using SixLabors.ImageSharp.Formats.Webp;
using Image = SixLabors.ImageSharp.Image;

namespace Atlas.Core.Utilities
{
    public class ImageInterface
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public BitmapImage LoadImage(string path, double width, double height = 0)
        {
            var image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/default.jpg"));

            //Check if path is empty or if the file exist
            if (path == "" || !File.Exists(path))
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
                    bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
                    bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;//bi;

                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    return image;
                }
            }
        }

        public async Task<string> ConvertToWebpAsync(byte[] imageArray, string imagePath)
        {
            string path = "";
            try
            {
                using var inStream = new MemoryStream(imageArray);

                using var image = await Image.LoadAsync(inStream);

                using var outStream = new MemoryStream();

                await image.SaveAsync(outStream, new WebpEncoder());

                await image.SaveAsWebpAsync($"{imagePath}.webp");
                image.Dispose();
                path = $"{imagePath}.webp";
            }
            catch (Exception ex) { Logger.Error(ex); }

            return path;
        }
    }
}
