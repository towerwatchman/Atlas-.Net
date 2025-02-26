using ABI.System;
using Atlas.UI.ViewModel;
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
using System.Windows.Shapes;

namespace Atlas.UI.Windows
{
    /// <summary>
    /// Interaction logic for GameDetailWindow.xaml
    /// </summary>
    public partial class GameDetailWindow : Window
    {
        public GameDetailWindow(GameViewModel game)
        {
            InitializeComponent();
            title.Text = game.Title;
            short_name.Text = game.ShortName;
            platform.Text = game.OS;
            engine.Text = game.Engine;
            developer.Text = game.Creator;
            release_date.Text = System.DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(game.ReleaseDate)).DateTime.ToString("yyyy-MM-dd");
            status.Text = game.Status;
            tags.Text = game.Tags.Replace(",", " , ");
            description.Text = game.Overview;
            category.Text = game.Category;
            latest_version.Text = game.LatestVersion;
            censored.Text = game.Censored;
            language.Text = game.Language;
            translations.Text = game.Translations;
            genre.Text = game.Genre;
            voice.Text = game.Voice;
            rating.Text = game.Rating;


        }


        #region Windows Buttons
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                this.BorderThickness = new System.Windows.Thickness(7);
            }
            else
            {
                WindowState = WindowState.Normal;
                this.BorderThickness = new System.Windows.Thickness(0);
            }

        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*
            double bannerX = (double)Application.Current.Resources["bannerX"];
            double bannerViewWidth = BannerView.ActualWidth;
            double rows = bannerViewWidth / bannerX;
            Console.WriteLine($"{bannerX} {bannerViewWidth} {rows}");

            Application.Current.Resources["Rows"] = (int)rows;
            Console.WriteLine(Application.Current.Resources["Rows"]);*/
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    this.DragMove();
                }));
        }
        #endregion
    }
}
