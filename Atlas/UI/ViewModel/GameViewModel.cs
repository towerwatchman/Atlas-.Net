// GameViewModel.cs
using Atlas.Core;
using NLog;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.IO;

namespace Atlas.UI.ViewModel
{
    public sealed class GameViewModel : INotifyPropertyChanged
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool _isUpdateAvailable;
        private BitmapSource _bannerImage;
        private static readonly ConcurrentDictionary<string, WeakReference<BitmapSource>> ImageCache =
            new ConcurrentDictionary<string, WeakReference<BitmapSource>>();
        private const int MaxCacheSize = 50;

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

        // Properties (unchanged except for SmallCapsule)
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
        public string SmallCapsule { get; set; }
        public string MainCapsule { get; set; }
        public string SiteUrl { get; private set; }
        public string[] Screens { get; private set; }
        public string Tags { get; private set; }
        public string Collection { get; private set; }
        public string CurrentSelectedVersion { get; set; }
        public string Rating { get; set; }
        public string LatestVersion { get; set; }
        public string Censored { get; set; }
        public string Language { get; set; }
        public string Genre { get; set; }
        public string ReleaseDate { get; set; }
        public string Translations { get; set; }
        public string Voice { get; internal set; }
        public string ShortName { get; internal set; }

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
                    catch { }
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

        public BitmapSource BannerImage
        {
            get => _bannerImage;
            set { _bannerImage = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Logger.Warn($"Property Changed: {propertyName}");
        }

        public async void RenderImage(ImageRenderMode mode)
        {
            if (BannerImage == null && !string.IsNullOrEmpty(SmallCapsule) && File.Exists(SmallCapsule))
            {
                string cacheKey = SmallCapsule + mode.ToString();
                if (ImageCache.TryGetValue(cacheKey, out WeakReference<BitmapSource> weakRef) &&
                    weakRef.TryGetTarget(out BitmapSource cachedImage))
                {
                    BannerImage = cachedImage;
                }
                else
                {
                    BannerImage = await Task.Run(() => RenderImageForMode(SmallCapsule, mode));
                    if (ImageCache.Count >= MaxCacheSize)
                    {
                        ImageCache.Clear();
                        Logger.Info("ImageCache cleared due to size limit");
                    }
                    ImageCache.TryAdd(cacheKey, new WeakReference<BitmapSource>(BannerImage));
                }
            }
        }

        public BitmapSource RenderImageOffThread(ImageRenderMode mode)
        {
            if (!string.IsNullOrEmpty(SmallCapsule) && File.Exists(SmallCapsule))
            {
                string cacheKey = SmallCapsule + mode.ToString();
                if (ImageCache.TryGetValue(cacheKey, out WeakReference<BitmapSource> weakRef) &&
                    weakRef.TryGetTarget(out BitmapSource cachedImage))
                {
                    Logger.Info($"Cache hit for {Title} ({cacheKey}), Cache size: {ImageCache.Count}");
                    return cachedImage;
                }
                else
                {
                    Logger.Info($"Rendering image for {Title} ({cacheKey})");
                    BitmapSource image = RenderImageForMode(SmallCapsule, mode);
                    if (image == null)
                    {
                        Logger.Warn($"Image rendering failed for {Title}");
                        return null;
                    }
                    if (ImageCache.Count >= MaxCacheSize)
                    {
                        ImageCache.Clear();
                        Logger.Info($"ImageCache cleared due to size limit, was {ImageCache.Count}");
                        GC.Collect(); // Force GC to test weak references
                        GC.WaitForPendingFinalizers();
                    }
                    ImageCache.TryAdd(cacheKey, new WeakReference<BitmapSource>(image));
                    Logger.Info($"Cached new image for {Title} ({cacheKey}), Cache size: {ImageCache.Count}");
                    return image;
                }
            }
            else
            {
                Logger.Warn($"Invalid SmallCapsule for {Title}: {SmallCapsule}");
                return null;
            }
        }

        public void ClearImage()
        {
            BannerImage = null;
        }

        private BitmapSource RenderImageForMode(string imagePath, ImageRenderMode mode)
        {
            BitmapImage originalImage = new BitmapImage();
            originalImage.BeginInit();
            originalImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            originalImage.CacheOption = BitmapCacheOption.OnLoad;
            originalImage.DecodePixelWidth = 537;
            originalImage.DecodePixelHeight = 251;
            originalImage.EndInit();
            originalImage.Freeze();

            switch (mode)
            {
                case ImageRenderMode.BlurredWithFeather:
                    return RenderCompositeImage(originalImage);
                case ImageRenderMode.Uniform:
                    return RenderSimpleImage(originalImage, Stretch.Uniform);
                case ImageRenderMode.Stretch:
                    return RenderSimpleImage(originalImage, Stretch.Fill);
                case ImageRenderMode.UniformToFill:
                    return RenderSimpleImage(originalImage, Stretch.UniformToFill);
                default:
                    return originalImage;
            }
        }

        private BitmapSource RenderCompositeImage(BitmapSource originalImage)
        {
            const double width = 537;
            const double height = 251;

            BitmapSource blurredBackground = CreateBlurredBackground(originalImage, width, height, blurFactor: 0.05);

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

                bool isTaller = height == scaledHeight;
                RenderTargetBitmap mask = CreateFeatherMask(width, height, isTaller);
                dc.PushOpacityMask(new ImageBrush(mask));
                dc.DrawImage(originalImage, new Rect(xOffset, yOffset, scaledWidth, scaledHeight));
                dc.Pop();
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(drawingVisual);
            rtb.Freeze();
            return rtb;
        }

        private BitmapSource RenderSimpleImage(BitmapSource originalImage, Stretch stretch)
        {
            const double width = 537;
            const double height = 251;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                if (stretch == Stretch.Uniform)
                {
                    double imgWidth = originalImage.PixelWidth;
                    double imgHeight = originalImage.PixelHeight;
                    double scale = Math.Min(width / imgWidth, height / imgHeight);
                    double scaledWidth = imgWidth * scale;
                    double scaledHeight = imgHeight * scale;
                    double xOffset = (width - scaledWidth) / 2;
                    double yOffset = (height - scaledHeight) / 2;
                    dc.DrawImage(originalImage, new Rect(xOffset, yOffset, scaledWidth, scaledHeight));
                }
                if (stretch == Stretch.Fill)
                {
                    double scaledHeight = height;
                    double scaledWidth = width;
                    double xOffset = (width - scaledWidth) / 2;
                    double yOffset = (height - scaledHeight) / 2;
                    dc.DrawImage(originalImage, new Rect(xOffset, yOffset, scaledWidth, scaledHeight));
                }
                if (stretch == Stretch.UniformToFill)
                {
                    double imgWidth = originalImage.PixelWidth;
                    double imgHeight = originalImage.PixelHeight;
                    double scale = (imgHeight / imgWidth);
                    double hDiff = height - imgHeight;
                    double wDiff = width - imgWidth;

                    if (hDiff < wDiff)
                    {
                        double scaledHeight = width * scale;
                        double scaledWidth = width;
                        double xOffset = (width - scaledWidth) / 2;
                        double yOffset = (height - scaledHeight) / 2;
                        dc.DrawImage(originalImage, new Rect(xOffset, yOffset, scaledWidth, scaledHeight));
                    }
                    else
                    {
                        double scaledHeight = height;
                        double scaledWidth = height / (imgHeight / imgWidth);
                        double xOffset = (width - scaledWidth) / 2;
                        double yOffset = (height - scaledHeight) / 2;
                        dc.DrawImage(originalImage, new Rect(xOffset, yOffset, scaledWidth, scaledHeight));
                    }
                }
            }
            rtb.Render(visual);
            rtb.Freeze();
            return rtb;
        }

        private BitmapSource CreateBlurredBackground(BitmapSource source, double width, double height, double blurFactor)
        {
            blurFactor = Math.Clamp(blurFactor, 0.05, 1.0);

            int smallWidth = (int)(width * blurFactor);
            int smallHeight = (int)(height * blurFactor);

            TransformedBitmap scaledDown = new TransformedBitmap(source, new ScaleTransform(blurFactor, blurFactor));
            RenderTargetBitmap smallBitmap = new RenderTargetBitmap(smallWidth, smallHeight, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual smallVisual = new DrawingVisual();
            using (DrawingContext dc = smallVisual.RenderOpen())
            {
                dc.DrawImage(scaledDown, new Rect(0, 0, smallWidth, smallHeight));
            }
            smallBitmap.Render(smallVisual);

            TransformedBitmap scaledUp = new TransformedBitmap(smallBitmap, new ScaleTransform(1 / blurFactor, 1 / blurFactor));
            RenderTargetBitmap finalBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual finalVisual = new DrawingVisual();
            using (DrawingContext dc = finalVisual.RenderOpen())
            {
                dc.DrawImage(scaledUp, new Rect(0, 0, width, height));
            }
            finalBitmap.Render(finalVisual);
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
                else
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
            }

            RenderTargetBitmap mask = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            mask.Render(maskVisual);
            mask.Freeze();
            return mask;
        }
    }
}