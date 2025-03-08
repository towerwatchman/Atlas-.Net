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
            BannerView.ItemsSource = ModelData.GameCollection;
            Logger.Info($"ItemsSource set with {ModelData.GameCollection.Count} items");

            // Example: Clear images for non-visible items periodically
            //BannerView.ScrollChanged += BannerView_ScrollChanged;
        }

        private void BannerView_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var visibleItems = BannerView.Items.Cast<GameViewModel>()
                .Skip((int)e.VerticalOffset)
                .Take((int)e.ViewportHeight)
                .ToList();

            foreach (var vm in ModelData.GameCollection.Except(visibleItems))
            {
                vm.ClearImage(); // Clear images for non-visible items
            }
        }
    }
}