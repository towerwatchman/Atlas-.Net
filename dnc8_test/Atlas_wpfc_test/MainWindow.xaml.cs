﻿using System.Text;
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
        private List<Game> GameList = new List<Game>();
        public MainWindow()
        {
            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(ListBoxItem), ListBoxItem.MouseLeftButtonUpEvent, new RoutedEventHandler(this.OnListBoxNavButtonUp));

            /*var gs = new List<Game>();
            for (int i = 0; i < 100; i++)
            {
                gs.Add(new Game { Creator = "Arcane Studios", Title = "Dishonored", Version = "1.0", Engine = "Unreal", Status = "Complete", ImageData = LoadImage("C:\\Users\\tower\\Downloads\\1699376987311.png") });
            }*/

            this.BannerView.ItemsSource = GameList;
            this.GameListBox.ItemsSource = GameList;
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
                //Set event handler to start parsing games once 
                batchImporter.btn_import.Click += Btn_import_Click;
                batchImporter.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                batchImporter.Show();
            }
        }

        private void Btn_import_Click(object sender, RoutedEventArgs e)
        {
            //This will take each game detail that was imported and change it into an actual game.
            //Store data in db, then show in the BannerView
            foreach(var GameDetail in GameScanner.GamesList)
            {
                GameList.Add(new Game { Creator = GameDetail.Creator, Title = GameDetail.Title, Version = GameDetail.Version, Engine = GameDetail.Engine, Status = "" , ImageData = LoadImage("") });
                BannerView.Items.Refresh();
                GameListBox.Items.Refresh();
            }

        }

        private BitmapImage? LoadImage(string path)
        {
            if (path == "")
            {
                return new BitmapImage( new Uri("pack://application:,,,/assets/Images/default.jpg"));
            }
            else
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double bannerX = (double)Application.Current.Resources["bannerX"];
            double bannerViewWidth = BannerView.ActualWidth;
            double rows = bannerViewWidth / bannerX;
            Console.WriteLine($"{bannerX} {bannerViewWidth} {rows}");

            Application.Current.Resources["Rows"] = (int)rows;
            Console.WriteLine(Application.Current.Resources["Rows"]);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}