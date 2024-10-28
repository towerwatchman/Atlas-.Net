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
            ShowVersions.Visibility = Visibility.Hidden;
        }

        private void BuildGameDetail()
        {
            if(CurrentGame != null)
            {
                ImageInterface image = new ImageInterface();
                var ImageData = image.LoadImage(CurrentGame.BannerPath, 1000, 250); //ImageInterface.LoadImage(CurrentGame.BannerPath, 1000);
                banner_main.Source = ImageData;
                Title.Content = CurrentGame.Title;
                //banner_left.Source = ImageData;
                //banner_right.Source = ImageData;
                //banner_background.Source = ImageData;
                CurrentVersion.Content = $"{CurrentGame.Versions[0].Version}";
                if(CurrentGame.Versions.Count > 1) 
                {
                    ShowVersions.Visibility = Visibility.Visible;
                }

            }
        }
    }
}
