// GameBannerControl.cs
using Atlas.UI.ViewModel;
using NLog;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Atlas.UI
{
    public enum ImageRenderMode
    {
        BlurredWithFeather,
        Uniform,
        Stretch,
        UniformToFill
    }

    public partial class GameBannerControl : UserControl
    {
        private CancellationTokenSource renderCts = new CancellationTokenSource();
        private DateTime lastRender = DateTime.MinValue;
        private readonly TimeSpan renderThrottle = TimeSpan.FromMilliseconds(100);
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static readonly DependencyProperty ImageRenderModeProperty =
            DependencyProperty.Register(
                nameof(ImageRenderMode),
                typeof(ImageRenderMode),
                typeof(GameBannerControl),
                new PropertyMetadata(ImageRenderMode.BlurredWithFeather, OnImageRenderModeChanged));

        public ImageRenderMode ImageRenderMode
        {
            get => (ImageRenderMode)GetValue(ImageRenderModeProperty);
            set => SetValue(ImageRenderModeProperty, value);
        }

        public GameBannerControl()
        {
            InitializeComponent();
            IsVisibleChanged += GameBannerControl_IsVisibleChanged;
            ImageRenderMode = ImageRenderMode.Uniform;
            Loaded += GameBannerControl_Loaded;
            Unloaded += GameBannerControl_Unloaded;
            DataContextChanged += GameBannerControl_DataContextChanged;
        }

        private static void OnImageRenderModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GameBannerControl control)
            {
                _ = control.TryRenderImageAsync();
            }
        }

        private void GameBannerControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                Logger.Info($"Loaded control for {vm.Title}");
                _ = TryRenderImageAsync();
            }
        }

        private void GameBannerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ClearImage();
                viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                Logger.Info($"Unloaded control for {viewModel.Title}, BannerImage cleared");
            }
            renderCts.Cancel();
        }
        private async void GameBannerControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && DataContext is GameViewModel vm)
            {
                Logger.Info($"Visibility changed to true for {vm.Title}");
                await TryRenderImageAsync();
            }
        }

        private async void GameBannerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is GameViewModel vm)
            {
                Logger.Info($"DataContext set for {vm.Title}");
                await TryRenderImageAsync();
            }

            if (e.OldValue is INotifyPropertyChanged oldViewModel)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            if (e.NewValue is INotifyPropertyChanged newViewModel)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameViewModel.SmallCapsule) || e.PropertyName == nameof(GameViewModel.BannerImage))
            {
                if (DataContext is GameViewModel vm)
                {
                    Logger.Info($"Property changed for {vm.Title}: {e.PropertyName}");
                    await TryRenderImageAsync();
                }
            }
        }

        private async Task TryRenderImageAsync()
        {
            if (!(DataContext is GameViewModel viewModel))
            {
                Logger.Warn("No GameViewModel in DataContext");
                return;
            }

            if (!IsLoaded || !IsVisibleInTree())
            {
                if (viewModel.BannerImage != null)
                {
                    viewModel.ClearImage();
                    Logger.Info($"Cleared image for {viewModel.Title} as control is not visible");
                }
                Logger.Info($"Control not loaded/visible for {viewModel.Title}, skipping render");
                return;
            }

            if (viewModel.BannerImage != null)
            {
                Logger.Info($"Image already set for {viewModel.Title}");
                return;
            }

            Logger.Info($"Attempting to render image for {viewModel.Title}");
            renderCts.Cancel();
            renderCts = new CancellationTokenSource();
            try
            {
                ImageRenderMode mode = ImageRenderMode;
                BitmapSource image = await Task.Run(() => viewModel.RenderImageOffThread(mode), renderCts.Token);
                if (image != null && !renderCts.IsCancellationRequested)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        viewModel.BannerImage = image;
                        Logger.Info($"Image set for {viewModel.Title}");
                    }, DispatcherPriority.Render);
                }
                else if (image == null)
                {
                    Logger.Warn($"Failed to render image for {viewModel.Title}");
                }
            }
            catch (TaskCanceledException)
            {
                Logger.Info($"Render cancelled for {viewModel.Title}");
            }
        }
        // Helper to check if control is in visual tree
        private bool IsVisibleInTree()
        {
            return Window.GetWindow(this) != null && IsVisible;
        }
    }
}