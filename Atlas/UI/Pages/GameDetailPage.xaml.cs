using Atlas.Core;
using Atlas.Core.Utilities;
using Atlas.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
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
        private GameViewModel CurrentGame { get; set; }
        public string ImageUriAnimated { get; set; }
        public GameDetailPage(GameViewModel game)
        {
            CurrentGame = game;
            InitializeComponent();
            BuildGameDetailAsync();
            ShowVersions.Visibility = Visibility.Hidden;
        }

        private async Task BuildGameDetailAsync()
        {
            if (CurrentGame != null)
            {

                BitmapImage ImageData = await ImageInterface.LoadImage(CurrentGame.RecordID, CurrentGame.BannerPath, 1000, 250);
                ImageData.Freeze();
                if(System.IO.Path.GetExtension(CurrentGame.BannerPath) == ".gif")
                {
                    ImageUriAnimated = CurrentGame.BannerPath;
                }
                else
                {
                    banner_main.Source = ImageData;
                    banner_background.Source = ImageData;
                }


                



                GameTitle.Content = CurrentGame.Title;
                //banner_left.Source = ImageData;
                //banner_right.Source = ImageData;
                //banner_background.Source = ImageData;
                CurrentVersion.Content = $"{CurrentGame.Versions[0].Version}";
                if (CurrentGame.Versions.Count > 1)
                {
                    ShowVersions.Visibility = Visibility.Visible;
                }

            }
        }
    }
}
