using Atlas.UI.ViewModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Atlas.UI
{
    /// <summary>
    /// Interaction logic for GameBannerControl.xaml
    /// </summary>
    public enum ImageRenderMode
    {
        BlurredWithFeather,
        Uniform,
        Stretch,
        UniformToFill,
        //UniformToFillCentered
    }
    public partial class GameBannerControl : UserControl
    {
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
            ImageRenderMode = ImageRenderMode.Uniform; // Set internally in constructor
            Loaded += GameBannerControl_Loaded;
            Unloaded += GameBannerControl_Unloaded;
            DataContextChanged += GameBannerControl_DataContextChanged;
        }
        /*private void UpdateAvailable_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button obButton = e.OriginalSource as Button;

            Process.Start(new ProcessStartInfo
            {
                FileName = obButton.Tag.ToString(),
                UseShellExecute = true
            });
        }*/
        private static void OnImageRenderModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GameBannerControl control)
            {
                _ = control.TryRenderImageAsync();
            }
        }

        private void GameBannerControl_Loaded(object sender, RoutedEventArgs e)
        {
            _ = TryRenderImageAsync();
        }

        private void GameBannerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ClearImage();
                viewModel.PropertyChanged -= ViewModel_PropertyChanged; // Unsubscribe
            }
        }

        private async void GameBannerControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await TryRenderImageAsync();
        }

        private async void GameBannerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await TryRenderImageAsync();

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
                await TryRenderImageAsync();
            }
        }

        private async Task TryRenderImageAsync()
        {
            if (!IsVisible || !(DataContext is GameViewModel viewModel) || viewModel.BannerImage != null)
            {
                return; // Skip if not visible or image already set
            }

            ImageRenderMode mode = ImageRenderMode;
            BitmapSource image = await Task.Run(() => viewModel.RenderImageOffThread(mode));
            if (image != null)
            {
                await Dispatcher.InvokeAsync(() => viewModel.BannerImage = image, DispatcherPriority.Background);
            }
        }
    }
}
