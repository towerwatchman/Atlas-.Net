using Atlas.Core;
using Atlas.UI.ViewModel;
using Atlas.UI.Windows;
using NLog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Atlas.UI.Pages

{
    /// <summary>
    /// Interaction logic for BannerViewPage.xaml
    /// </summary>
    public partial class BannerViewPage : Page
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool isGameDetailShown = false;
        public BannerViewPage()
        {
            InitializeComponent();
            DataContext = this;
            this.Visibility = Visibility.Hidden;

            //BannerView.ItemsSource = ModelData.GameCollection;
            Logger.Info($"ItemsSource set with {ModelData.GameCollection.Count} items");

            string f95XamlPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "banners", $"{Atlas.Core.Settings.Config.BannerTheme}.xaml");


            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                InterfaceHelper.SetBannerTemplate(Atlas.Core.Settings.Config.BannerTheme, f95XamlPath);
                BannerView.Items.Refresh();
                this.Visibility = Visibility.Visible;
                BannerView.UpdateLayout();
            }, DispatcherPriority.Background);
            /*
            Loaded += (s, e) =>
            {
                BannerView.UpdateLayout();
                Logger.Debug($"BannerView ActualHeight after Loaded: {BannerView.ActualHeight}, ActualWidth: {BannerView.ActualWidth}");
                if (BannerView.Items.Count > 0)
                {
                    var firstItem = BannerView.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement;
                    if (firstItem != null)
                    {
                        Logger.Debug($"First item Type: {firstItem.GetType().Name}");
                        Logger.Debug($"First item ActualHeight: {firstItem.ActualHeight}, ActualWidth: {firstItem.ActualWidth}");
                        firstItem.UpdateLayout();
                        Logger.Debug($"First item ActualHeight after UpdateLayout: {firstItem.ActualHeight}, ActualWidth: {firstItem.ActualWidth}");
                        var contentPresenter = FindVisualChild<ContentPresenter>(firstItem);
                        if (contentPresenter != null)
                        {
                            Logger.Debug($"ContentTemplate: {contentPresenter.ContentTemplate != null}");
                            var content = contentPresenter.Content as GameViewModel;
                            Logger.Debug($"Content: {content?.Title ?? "null"}");
                            Logger.Debug("First item visual tree:");
                            DumpVisualTree(firstItem, 0);
                        }
                        else
                        {
                            Logger.Warn("ContentPresenter not found in first item");
                        }
                    }
                    else
                    {
                        Logger.Warn("First item container is null");
                    }
                }
            };


        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T found) return found;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private void DumpVisualTree(DependencyObject obj, int level)
        {
            if (obj == null) return;
            Logger.Debug(new string(' ', level * 2) + $"{obj.GetType().Name} (Visibility: {obj.GetValue(FrameworkElement.VisibilityProperty)}, Opacity: {obj.GetValue(FrameworkElement.OpacityProperty)})");
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                DumpVisualTree(VisualTreeHelper.GetChild(obj, i), level + 1);
        }*/
        }
    }
}