using Atlas.UI.Pages.Settings;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Atlas.UI.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            this.SettingsFrame.Content = new InterfacePage();//GameDetailPage((GameViewModel)GameListBox.SelectedItem);
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

        private void OnListBoxNavButtonUp(object sender, MouseButtonEventArgs e)
        {
            var Item = (ListBoxItem)sender;
            //Console.WriteLine(Item.Name);
            Console.Out.WriteLine(Item.Name);

            if (Item.Name.ToString() == "Interface")
            {
                this.SettingsFrame.Content = new InterfacePage();
            }

            if (Item.Name.ToString() == "Library")
            {
                this.SettingsFrame.Content = new LibraryPage();
            }
        }
    }
}
