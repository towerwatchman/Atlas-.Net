using Atlas.Core;
using Atlas.UI.ViewModel;
using Atlas.UI.Windows;
using NLog;
using System.Windows;
using System.Windows.Controls;

namespace Atlas.UI.Pages

{
    /// <summary>
    /// Interaction logic for BannerViewPage.xaml
    /// </summary>
    public partial class BannerViewPage : Page
    {
        private bool isGameDetailShown = false;
        public BannerViewPage()
        {
            InitializeComponent();            
        }
    }
}
