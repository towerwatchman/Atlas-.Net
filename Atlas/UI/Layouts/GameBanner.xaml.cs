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
    /// Interaction logic for GameBanner.xaml
    /// </summary>
    public partial class GameBanner : UserControl
    {
        public GameBanner()
        {
            InitializeComponent();
            IsVisibleChanged += UserControl_IsVisibleChanged;
            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
            DataContextChanged += UserControl_DataContextChanged;
        }
        private void UpdateAvailable_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Button obButton = e.OriginalSource as Button;

            Process.Start(new ProcessStartInfo
            {
                FileName = obButton.Tag.ToString(),
                UseShellExecute = true
            });
        }

        //---------------------------------------------------------
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TryRenderImage();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GameViewModel viewModel)
            {
                viewModel.ClearImage();
            }
        }

        private async void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            await TryRenderImageAsync();
        }

        private async void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
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

        private void TryRenderImage() // Synchronous version for Loaded
        {
            if (IsVisible && DataContext is GameViewModel viewModel)
            {
                viewModel.RenderImage();
            }
        }

        private async Task TryRenderImageAsync() // Async version for events
        {
           await Dispatcher.Invoke(async () =>
            {
                if (!IsVisible || !(DataContext is GameViewModel viewModel)) return;
           

            // Render on background thread and assign on UI thread
            BitmapSource image = await Task.Run(() => viewModel.RenderImageOffThread());
            if (image != null)
            {
                await Dispatcher.InvokeAsync(() => viewModel.BannerImage = image, DispatcherPriority.Background);
            } 
            });
        }
    }
}
