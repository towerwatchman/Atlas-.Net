using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Windows.Media.Imaging;
using Image = SixLabors.ImageSharp.Image;

namespace Atlas.Core.Utilities
{
    public class ImageInterface
    {
        public static Dictionary<int, BitmapImage> _cache = new Dictionary<int, BitmapImage>();

        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public BitmapImage LoadImages(string path, double width, double height = 0)
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
                    bitmapImage.CacheOption = BitmapCacheOption.Default;
                    bitmapImage.CreateOptions = BitmapCreateOptions.None;
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

        public async Task<string> ConvertToWebpAsync(byte[] imageArray, string imagePath, bool createThumb)
        {
            string path = "";
            try
            {
                using var inStream = new MemoryStream(imageArray);
                inStream.Position = 0;

                using (Image image = await Image.LoadAsync(inStream))
                {

                    double max_size = 616;

                    if (image.Width > image.Height)
                    {
                        double height = (max_size / image.Width) * image.Height;
                        image.Mutate(x => x.Resize(Convert.ToInt32(max_size), Convert.ToInt32(height), KnownResamplers.Lanczos3));
                    }
                    else
                    {
                        double width = (max_size / image.Height) * image.Width;
                        image.Mutate(x => x.Resize(Convert.ToInt32(width), Convert.ToInt32(max_size), KnownResamplers.Lanczos3));
                    }

                    using var outStream = new MemoryStream();

                    await image.SaveAsync(outStream, new WebpEncoder());
                    await image.SaveAsWebpAsync($"{imagePath}.webp");
                    path = $"{imagePath}.webp";

                    if (createThumb)
                    {
                        await CreateThumb(image, imagePath);
                    }
                }
            }
            catch (Exception ex) { Logger.Error(ex); }

            return path;
        }

        public async Task<Task> CreateThumb(Image image, string imagePath)
        {
            double max_size = 20;
            try
            {
                if (image.Width > image.Height)
                {
                    double height = (max_size / image.Width) * image.Height;
                    image.Mutate(x => x.Resize(Convert.ToInt32(max_size), Convert.ToInt32(height), KnownResamplers.Lanczos3));
                }
                else
                {
                    double width = (max_size / image.Height) * image.Width;
                    image.Mutate(x => x.Resize(Convert.ToInt32(width), Convert.ToInt32(max_size), KnownResamplers.Lanczos3));
                }

                //using var outStream = new MemoryStream();
                //await image.SaveAsync(outStream, new WebpEncoder());

                await image.SaveAsWebpAsync($"{imagePath}_thumb.webp");
            }
            catch (Exception ex) { Logger.Error(ex); }

            return Task.CompletedTask;
        }

        public static async Task<BitmapSource> LoadImageAsync(int id, string bannerPath, double imageRenderWidth, double imageRenderHeight = 0)
        {
            //Logger.Warn($"Getting image for id: {id}");
            //try to get image from cache

            //BitmapImage bitmapImage;
            Uri uri = new Uri("pack://application:,,,/Assets/Images/default.jpg");
            string path = "";// "pack://application:,,,/Assets/Images/default.jpg";


            try
            {

                if (File.Exists(bannerPath))
                {
                    path = bannerPath;
                }
                else
                {
                    return null;// new BitmapImage(uri);
                }


                BitmapImage bitmapImage;

                //Logger.Error($"ID {id} request for image");

                //Logger.Debug($"Loading Image from disk for: {id}");
                byte[] image;
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    image = new byte[stream.Length];
                    await stream.ReadAsync(image, 0, (int)stream.Length);
                }
                //= await System.IO.File.ReadAllBytes(path);

                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                //bitmapImage.UriSource = uri;
                bitmapImage.StreamSource = new MemoryStream(image);
                //bitmapImage.DecodePixelWidth = (int)imageRenderWidth;
                bitmapImage.CacheOption = BitmapCacheOption.Default;
                bitmapImage.CreateOptions = BitmapCreateOptions.None;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                image = null;
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return null;
            }
        }
    }
}
