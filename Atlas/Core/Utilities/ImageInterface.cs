using Atlas.Core.Database;
using Atlas.UI;
using Atlas.UI.ViewModel;
using NLog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Windows.Media.Imaging;
using System.Windows;
using Windows.Media.Protection.PlayReady;
using Windows.Graphics.Imaging;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System.Drawing;
using Image = SixLabors.ImageSharp.Image;
using System.Drawing.Imaging;

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

        public async Task<string> ConvertToWebpAsync(byte[] imageBytes, string outputPath, int maxWidth, bool createThumb = false)
        {
            string path = "";
            try
            {
                System.Drawing.Image _image = null;
                using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                {
                    _image = System.Drawing.Image.FromStream(memoryStream);
                }
                MemoryStream ms = new MemoryStream();
                _image.Save(ms, _image.RawFormat);
                ms.Position = 0; // Reset the stream position to the beginning

                using (Image image = Image.Load(ms))
                {
                    //imageStream.Position = 0; // Ensure the stream position is set to 0 before reading

                    // Resize the image to a max width of 600px while maintaining aspect ratio
                    if (image.Width > maxWidth)
                    {
                        image.Mutate(x => x.Resize(maxWidth, 0));
                    }

                    // Save the image in WebP format
                    using (FileStream output = File.OpenWrite($"{outputPath}.webp"))
                    {
                       // image.Save(output, new WebpEncoder());
                    }
                    path = $"{outputPath}.webp";
                }
                /*using var inStream = new MemoryStream(imageArray);
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
                }*/
            }
            catch (Exception ex) { Logger.Error(ex); return null; }

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
                        await Core.Network.NetworkHelper.DownloadFileAsync(bannerUrl, banner_path, 200);

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
                                    ModelData.GameCollection[index].BannerPath = banner_path;
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

        public static async Task<bool> ResizeAndSaveAsWebP(Bitmap originalImage, string outputPath,
     int max_size = 660, int? newHeight = null)
        {
            try
            {
                // ... (dimension calculation remains the same)

                using (var ms = new MemoryStream())
                {
                    originalImage.Save(ms, ImageFormat.Bmp);
                    ms.Position = 0;

                    using (var image = await Image.LoadAsync(ms))
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
                        await image.SaveAsWebpAsync(outputPath);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                originalImage.Dispose();
            }
        }
    }
}
