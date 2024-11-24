using Atlas.Core.Utilities;
using Atlas.UI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

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
            BuildGameDetail();
            ShowVersions.Visibility = Visibility.Hidden;
        }

        private void BuildGameDetail()
        {
            if (CurrentGame != null)
            {
                BitmapSource ImageData = ImageInterface.LoadImage(CurrentGame.RecordID, CurrentGame.BannerPath, 1000, 250);
                if (ImageData != null)
                {
                    ImageData.Freeze();
                    if (System.IO.Path.GetExtension(CurrentGame.BannerPath) == ".gif")
                    {
                        AnimationBehavior.SetSourceUri(AnimatedBanner, new System.Uri(CurrentGame.BannerPath));
                    }
                    else
                    {
                        banner_main.Source = ImageData;

                    }
                    banner_background.Source = ImageData;
                }
                GameTitle.Content = CurrentGame.Title;

                CurrentVersion.Content = $"{CurrentGame.Versions[0].Version}";
                if (CurrentGame.Versions.Count > 1)
                {
                    ShowVersions.Visibility = Visibility.Visible;
                }

            }
        }
    }
}
