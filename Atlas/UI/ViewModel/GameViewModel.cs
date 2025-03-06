using Atlas.Core;
using NLog;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Atlas.UI.ViewModel
{
    public sealed class GameViewModel : INotifyPropertyChanged
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public GameViewModel(Game game)
        {
            RecordID = game.RecordID;
            AtlasID = game.AtlasID;
            F95ID = game.F95ID;
            Title = game.Title;
            Creator = game.Creator;
            Engine = game.Engine;
            Versions = game.Versions;
            Status = game.Status;
            Likes = game.Likes;
            Favorites = game.Favorites;
            Views = game.Views;
            Category = game.Category;
            Overview = game.Overview;
            OS = game.OS;
            IsFavorite = game.IsFavorite;
            SmallCapsule = game.SmallCapsule;
            MainCapsule = game.MainCapsule;
            SiteUrl = game.SiteUrl;
            Screens = game.Screens;
            Tags = game.Tags;
            Collection = game.Collection;
            CurrentSelectedVersion = game.CurrentSelectedVersion;
            Rating = game.Rating;
            LatestVersion = game.LatestVersion;
            Censored = game.Censored;
            Language = game.Language;
            Genre = game.Genre;
            ReleaseDate = game.ReleaseDate;
            Translations = game.Translations;
            Voice = game.Voice;
            ShortName = game.ShortName;

        }
        private bool _isUpdateAvailable;
        //private string _smallCapsule;
        private BitmapSource _bannerImage;
        private static readonly ConcurrentDictionary<string, BitmapSource> ImageCache = new ConcurrentDictionary<string, BitmapSource>();

        public int RecordID { get; private set; }
        public int AtlasID { get; private set; }
        public int F95ID { get; private set; }
        public string Title { get; set; }
        public string Creator { get; set; }
        public string Engine { get; set; }
        public List<GameVersion> Versions { get; set; }
        public string Status { get; set; }
        public string Likes { get; private set; }
        public string Favorites { get; private set; }
        public string Views { get; private set; }
        public string Category { get; private set; }
        public string Overview { get; private set; }
        public string OS { get; private set; }
        public bool IsFavorite { get; private set; }
        /*public string SmallCapsule
        {
            get => _smallCapsule;
            set { _smallCapsule = value; OnPropertyChanged(); }
        }*/
        public string SmallCapsule
        {
            get; set;
        }
        public string MainCapsule { get; set; }
        public string ImageUriAnimated { get; set; }
        public string SiteUrl { get; private set; }
        public string[] Screens { get; private set; }
        public string Tags { get; private set; }
        public string Collection { get; private set; }
        public string CurrentSelectedVersion { get; set; }
        public string Rating { get; set; }
        public string LatestVersion { get; set; }
        public string Censored { get; set; }
        public string Language { get; set; }

        public bool IsUpdateAvailable
        {
            get
            {
                Task.Run(() =>
                {
                    bool isUpdateAvailable = false;
                    int latest = 0;
                    try
                    {
                        latest = Convert.ToInt32(Regex.Replace(LatestVersion, "[^0-9]", ""));
                    }
                    catch
                    {

                    }
                    foreach (var version in Versions)
                    {
                        int current = 0;
                        try
                        {
                            current = Convert.ToInt32(Regex.Replace(version.Version, "[^0-9]", ""));
                        }
                        catch { }
                        if (latest > current)
                        {
                            isUpdateAvailable = true;
                        }
                        else
                        {
                            isUpdateAvailable = false;
                            break;
                        }
                    }
                    if (isUpdateAvailable != _isUpdateAvailable)
                    {
                        _isUpdateAvailable = isUpdateAvailable;
                        OnPropertyChanged(nameof(IsUpdateAvailable));
                    }
                });

                return _isUpdateAvailable;
            }
            set
            {
                _isUpdateAvailable = value;
                OnPropertyChanged(nameof(IsUpdateAvailable));
            }
        }
        /*public BitmapSource BannerImage
        {
            get
            {
                if (_bannerImage == null)
                {
                    //BannersInView.Add(RecordID);
                    //return _bannerImage;
                    Task.Run(async () =>
                    {
                        BitmapSource bi = await ImageInterface.LoadImageAsync(RecordID, SmallCapsule, Atlas.Core.Settings.Config.ImageRenderWidth);
                        if (bi != null)
                        {

                            bi.Freeze();
                            _bannerImage = bi;
                            bi = null;

                            OnPropertyChanged("BannerImage");

                            //Logger.Warn($"Loaded Image for id: {RecordID}");
                        }
                    });

                    return _bannerImage;
                }
                return _bannerImage;
            }
            set { _bannerImage = value; }
        }*/

        public string Genre { get; set; }
        public string ReleaseDate { get; set; }
        public string Translations { get; set; }
        public string Voice { get; internal set; }
        public string ShortName { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Logger.Warn($"Property Changed:{propertyName}");
        }
        public BitmapSource BannerImage
        {
            get => _bannerImage;
            set { _bannerImage = value; OnPropertyChanged(); }
        }
        public void RenderImage() // For synchronous calls
        {
            if (BannerImage == null && !string.IsNullOrEmpty(SmallCapsule) && File.Exists(SmallCapsule))
            {
                if (ImageCache.TryGetValue(SmallCapsule, out BitmapSource cachedImage))
                {
                    BannerImage = cachedImage;
                }
                else
                {
                    BannerImage = RenderCompositeImageFromPath(SmallCapsule);
                    ImageCache.TryAdd(SmallCapsule, BannerImage);
                }
            }
        }
        public BitmapSource RenderImageOffThread() // For async calls
        {
            if (BannerImage == null && !string.IsNullOrEmpty(SmallCapsule) && File.Exists(SmallCapsule))
            {
                if (ImageCache.TryGetValue(SmallCapsule, out BitmapSource cachedImage))
                {
                    return cachedImage;
                }
                else
                {
                    BitmapSource image = RenderCompositeImageFromPath(SmallCapsule);
                    ImageCache.TryAdd(SmallCapsule, image);
                    return image;
                }
            }
            return null;
        }
        public void ClearImage()
        {
            BannerImage = null; // Cache persists, so we don’t need to reload
        }

        private BitmapSource RenderCompositeImageFromPath(string imagePath)
        {
            BitmapImage originalImage = new BitmapImage();
            originalImage.BeginInit();
            originalImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            originalImage.CacheOption = BitmapCacheOption.OnLoad;
            originalImage.EndInit();
            originalImage.Freeze(); // Freeze to make it thread-safe

            return RenderCompositeImage(originalImage);
        }

        private BitmapSource RenderCompositeImage(BitmapSource originalImage)
        {
            if (originalImage == null) return null;

            const double width = 537;
            const double height = 251;

            // Simplify blur by scaling down only once
            BitmapSource blurredBackground = CreateBlurredBackground(originalImage, width, height);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                dc.DrawImage(blurredBackground, new Rect(0, 0, width, height));

                double imgWidth = originalImage.PixelWidth;
                double imgHeight = originalImage.PixelHeight;
                double scale = Math.Min(width / imgWidth, height / imgHeight);
                double scaledWidth = imgWidth * scale;
                double scaledHeight = imgHeight * scale;
                double xOffset = (width - scaledWidth) / 2;
                double yOffset = (height - scaledHeight) / 2;

                RenderTargetBitmap mask = CreateFeatherMask(width, height, imgHeight > imgWidth);
                dc.PushOpacityMask(new ImageBrush(mask));
                dc.DrawImage(originalImage, new Rect(xOffset, yOffset, scaledWidth, scaledHeight));
                dc.Pop();
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            rtb.Freeze(); // Freeze for thread safety
            return rtb; // Simplified: ApplyBlur removed for performance
        }

        private BitmapSource CreateBlurredBackground(BitmapSource source, double width, double height)
        {
            // Simplified blur: scale down and up once
            double scaleFactor = 0.25; // Lower scale factor for faster blur
            int smallWidth = (int)(width * scaleFactor);
            int smallHeight = (int)(height * scaleFactor);

            TransformedBitmap scaledDown = new TransformedBitmap(source, new ScaleTransform(scaleFactor, scaleFactor));
            RenderTargetBitmap smallBitmap = new RenderTargetBitmap(smallWidth, smallHeight, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual smallVisual = new DrawingVisual();
            using (DrawingContext dc = smallVisual.RenderOpen()) // Correct: RenderOpen on DrawingVisual
            {
                dc.DrawImage(scaledDown, new Rect(0, 0, smallWidth, smallHeight));
            }
            smallBitmap.Render(smallVisual); // Render DrawingVisual to RenderTargetBitmap

            TransformedBitmap scaledUp = new TransformedBitmap(smallBitmap, new ScaleTransform(1 / scaleFactor, 1 / scaleFactor));
            RenderTargetBitmap finalBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual finalVisual = new DrawingVisual();
            using (DrawingContext dc = finalVisual.RenderOpen()) // Correct: RenderOpen on DrawingVisual
            {
                dc.DrawImage(scaledUp, new Rect(0, 0, width, height));
            }
            finalBitmap.Render(finalVisual); // Render DrawingVisual to RenderTargetBitmap
            finalBitmap.Freeze();
            return finalBitmap;
        }

        private RenderTargetBitmap CreateFeatherMask(double width, double height, bool isTaller)
        {
            DrawingVisual maskVisual = new DrawingVisual();
            using (DrawingContext dc = maskVisual.RenderOpen())
            {
                if (isTaller)
                {
                    LinearGradientBrush gradient = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(0, 1),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(Colors.Transparent, 0),
                            new GradientStop(Colors.White, 0.1),
                            new GradientStop(Colors.White, 0.9),
                            new GradientStop(Colors.Transparent, 1)
                        }
                    };
                    dc.DrawRectangle(gradient, null, new Rect(0, 0, width, height));
                }
                else
                {
                    LinearGradientBrush gradient = new LinearGradientBrush
                    {
                        StartPoint = new Point(0, 0),
                        EndPoint = new Point(1, 0),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(Colors.Transparent, 0),
                            new GradientStop(Colors.White, 0.1),
                            new GradientStop(Colors.White, 0.9),
                            new GradientStop(Colors.Transparent, 1)
                        }
                    };
                    dc.DrawRectangle(gradient, null, new Rect(0, 0, width, height));
                }
            }

            RenderTargetBitmap mask = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            mask.Render(maskVisual);
            mask.Freeze();
            return mask;
        }
    }
}
