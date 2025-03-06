using Atlas.Core.Database;
using Atlas.UI;
using Atlas.UI.ViewModel;
using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using Windows.Media.Protection.PlayReady;
using Windows.Graphics.Imaging;
using SkiaSharp;
using System.Drawing;
using System.Drawing.Imaging;
using ImageMagick;
using NLog.Config;

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

        public static async Task DownloadImages(GameDetails gameDetail, bool DownloadPreviews = false)
        {
            if (gameDetail.AtlasID == null || gameDetail.AtlasID == "") { Logger.Warn("Unable to find Atlas ID to download images"); return; } //exit if no image found
            try
            {
                //Get Banner Path for database
                //Logger.Info(game.Title);
                string banner_path = SQLiteInterface.GetBannerPath(gameDetail.RecordID.ToString());
                //check if banner already exist
                if ((banner_path == string.Empty && Convert.ToInt32(gameDetail.AtlasID) > -1) || !File.Exists(banner_path))
                {
                    Logger.Info($"Downloading images id:{gameDetail.RecordID} name:{gameDetail.Title}");


                    //Run below in a new task that is awaited
                    await Task.Run(async () =>
                    {
                        string bannerUrl = SQLiteInterface.GetBannerUrl(gameDetail.AtlasID.ToString());
                        Directory.CreateDirectory(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\images", gameDetail.RecordID.ToString()));
                        byte[] ImageArray = null;

                        banner_path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\images", gameDetail.RecordID.ToString(), Path.GetFileName(bannerUrl));
                        await Core.Networking.NetworkHelper.DownloadFileAsync(bannerUrl, banner_path, 200);

                        /*if (Path.GetExtension(bannerUrl) == ".gif")
                        {*/
                        //banner_path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\images", game.RecordID.ToString(), Path.GetFileName(bannerUrl));
                        //await Core.Network.NetworkInterface.DownloadFileAsync(bannerUrl, banner_path, 200);
                        /*}
                        else
                        {*/
                        //banner_path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\images", game.RecordID.ToString(), Path.GetFileNameWithoutExtension(bannerUrl));
                        //ImageArray = await Core.Network.NetworkInterface.DownloadBytesAsync(bannerUrl, banner_path, 2000);
                        //}

                        //Atlas.Core.Network.NetworkInterface networkInterface = new Core.Network.NetworkInterface();

                        //will need to implement this later
                        if (ImageArray != null)
                        {
                            //ConvertImage to webp
                            //ImageInterface image = new ImageInterface();
                            //banner_path = image.ConvertToWebpAsync(ImageArray, banner_path, false).Result;

                        }
                        //update banner table
                        if (File.Exists(banner_path))
                        {
                            SQLiteInterface.UpdateBanners(Convert.ToInt32(gameDetail.RecordID), banner_path, "banner");

                            Logger.Info($" Updated Banner Images for: {gameDetail.Title.ToString()}");
                            //Find Game in gamelist and set the banner to it
                            BitmapSource img = await ImageInterface.LoadImageAsync(Convert.ToInt32(gameDetail.RecordID),
                                        bannerUrl == "" ? "" : banner_path,
                                        Atlas.Core.Settings.Config.ImageRenderWidth,
                                        Atlas.Core.Settings.Config.ImageRenderHeight);
                            //Freeze Image so it can update main UI thread
                            img.Freeze();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == Convert.ToInt32(gameDetail.RecordID)).FirstOrDefault();
                                var index = ModelData.GameCollection.IndexOf(gameObj);

                                if (gameObj != null)
                                {
                                    ModelData.GameCollection[index].BannerImage = img;
                                    ModelData.GameCollection[index].SmallCapsule = banner_path;
                                    gameObj.OnPropertyChanged("BannerImage");
                                }
                            });
                        }

                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                    });
                    //Set a default waiting period for downloading images between 200ms and 1s
                    Random random = new Random();
                    System.Threading.Thread.Sleep(random.Next(1000, 2000));
                }

                string[] screens = SQLiteInterface.getScreensUrlList(gameDetail.RecordID.ToString());

                //if()
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        public static void CreateAtlasAssets(string path, string filename)
        {

        }

        public static async Task<bool> ConvertToWebpAsync(byte[] imageBytes, uint quality, uint maxDimension, string outFile)
        {
            using (var image = new MagickImage(imageBytes))
            {
                try
                {
                    //Check if file type before continueing
                    string type = DetectImageFormat(imageBytes);
                    Logger.Info($"{type} image format detected");
                    // Get original dimensions
                    int width = (int)image.Width;
                    int height = (int)image.Height;
                    //Check for a valid image format
                    if (type != "")
                    {
                        //Save gif if available
                        if (type == "gif")
                        {
                            byte[] gifBytes = image.ToByteArray();
                            await File.WriteAllBytesAsync(outFile.Replace(".webp", ".gif"), gifBytes);
                        }
                        // Resize if necessary
                        if ((width > maxDimension || height > maxDimension) && maxDimension > 0)
                        {
                            if (width > height)
                            {
                                // Scale based on width
                                image.Resize(maxDimension, 0); // Height will scale proportionally, 0 maintains aspect ratio
                            }
                            else
                            {
                                // Scale based on height
                                image.Resize(0, maxDimension); // Width will scale proportionally, 0 maintains aspect ratio
                            }
                        }

                        // Set WebP format and quality
                        image.Format = MagickFormat.WebP;
                        image.Quality = quality;

                        // Optionally configure WebP-specific settings
                        image.Settings.SetDefine("webp:lossless", "false"); // Use "true" for lossless
                                                                            // image.Settings.SetDefine("webp:compression-level", "6"); // 0-6, higher = better compression

                        // Convert to WebP and return bytes
                        byte[] webpBytes = image.ToByteArray();

                        await File.WriteAllBytesAsync(outFile, webpBytes);
                        Logger.Info($"Saved as: {outFile}");
                        return true;
                    }
                    else
                    {
                        Logger.Warn("Image Type was not in expected format");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    return false;
                }
            }
        }

        static string DetectImageFormat(byte[] imageBytes)
        {
            // Check if we have enough bytes to analyze (increased to 14 for TGA)
            if (imageBytes.Length < 14)
                return string.Empty;

            // JPEG: FF D8 FF
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 && imageBytes[2] == 0xFF)
                return "jpg";

            // PNG: 89 50 4E 47
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
                imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                return "png";

            // GIF: 47 49 46 38
            if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49 &&
                imageBytes[2] == 0x46 && imageBytes[3] == 0x38)
                return "gif";

            // BMP: 42 4D
            if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
                return "bmp";

            // WebP: 52 49 46 46 (RIFF) ... 57 45 42 50 (WEBP)
            if (imageBytes[0] == 0x52 && imageBytes[1] == 0x49 &&
                imageBytes[2] == 0x46 && imageBytes[3] == 0x46 &&
                imageBytes[8] == 0x57 && imageBytes[9] == 0x45 &&
                imageBytes[10] == 0x42 && imageBytes[11] == 0x50)
                return "webp";

            // AVIF: Usually starts with ftypavif in bytes 4-11
            if (imageBytes[4] == 0x66 && imageBytes[5] == 0x74 &&
                imageBytes[6] == 0x79 && imageBytes[7] == 0x70 &&
                imageBytes[8] == 0x61 && imageBytes[9] == 0x76 &&
                imageBytes[10] == 0x69 && imageBytes[11] == 0x66)
                return "avif";

            // PBM (Portable Bitmap, ASCII): 50 31 (P1)
            if (imageBytes[0] == 0x50 && imageBytes[1] == 0x31)
                return "pbm";

            // TIFF: 49 49 2A 00 (little-endian) or 4D 4D 00 2A (big-endian)
            if ((imageBytes[0] == 0x49 && imageBytes[1] == 0x49 && imageBytes[2] == 0x2A && imageBytes[3] == 0x00) ||
                (imageBytes[0] == 0x4D && imageBytes[1] == 0x4D && imageBytes[2] == 0x00 && imageBytes[3] == 0x2A))
                return "tiff";

            // TGA (Truevision TGA): Basic header check
            if (imageBytes[0] <= 0x01 && imageBytes[1] <= 0x0D &&
                (imageBytes[2] == 0x01 || imageBytes[2] == 0x02 || imageBytes[2] == 0x03 ||
                 imageBytes[2] == 0x09 || imageBytes[2] == 0x0A || imageBytes[2] == 0x0B))
                return "tga";

            // QOI (Quite OK Image): 71 6F 69 66 (qoif)
            if (imageBytes[0] == 0x71 && imageBytes[1] == 0x6F &&
                imageBytes[2] == 0x69 && imageBytes[3] == 0x66)
                return "qoi";

            // Default to .jpg if unknown
            return "unknown";
        }

    }
}
