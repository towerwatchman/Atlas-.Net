using Atlas.Core;
using Atlas.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Atlas.UI.Pages
{
    /// <summary>
    /// Interaction logic for GameDetailPage.xaml
    /// </summary>
    public partial class GameDetailPage : Page
    {
        private Game CurrentGame { get; set; }
        public GameDetailPage(Game game)
        {
            CurrentGame = game;
            InitializeComponent();
            BuildGameDetail();
        }

        private void BuildGameDetail()
        {
            if(CurrentGame != null)
            {
                var ImageData = ImageInterface.LoadImage(CurrentGame.BannerPath, 1000);
                banner_main.Source = ImageData;
                banner_background.Source = ImageData;
            }
        }
    }
}
