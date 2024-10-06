using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Atlas.UI.Importer;
using Atlas.Core;
using System.IO;

namespace Atlas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(ListBoxItem), ListBoxItem.MouseLeftButtonUpEvent, new RoutedEventHandler(this.OnListBoxNavButtonUp));

            var gs = new List<Game>();
            for (int i = 0; i < 1000; i++)
            {
                gs.Add(new Game { Creator = "Arcane Studios", Title = "Dishonored", Version = "1.0", Engine = "Unreal", Status = "Complete", ImageData = LoadImage("C:\\Users\\tower\\Downloads\\1699376987311.png") });
            }

            this.BannerView.ItemsSource = gs;
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }

        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void OnListBoxNavButtonUp(object sender, RoutedEventArgs e)
        {
            var Item = (ListBoxItem)sender;
            Console.WriteLine(Item.Name);
            if (Item.Name.ToString() == "Import")
            {
                BatchImporter batchImporter = new BatchImporter();
                batchImporter.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                batchImporter.Show();
            }
        }
        private BitmapImage? LoadImage(string path)
        {
            try
            {
                var uri = new Uri(path);
                return new BitmapImage(uri);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}