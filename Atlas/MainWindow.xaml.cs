using Atlas.Core;
using Atlas.Core.Database;
using Atlas.UI.Importer;
using NLog;
using SharpVectors.Dom.Css;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

            this.BannerView.ItemsSource = GameList;
            this.GameListBox.ItemsSource = GameList;

            //Hide context menu
            cmGame.Visibility = Visibility.Hidden;
            BannerView.MouseUp += BannerView_MouseUp;
        }

        #region Banner Left Click
        private void BannerView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.BannerView.ItemsSource != null && this.BannerView.Items.Count >0)
            {
                
                Game game = BannerView.SelectedItem as Game;
                if (game != null)
                {
                    cmGame.Visibility = Visibility.Visible;
                    miPlay.ItemsSource = null;
                    miPlay.Items.Clear();

                    List<MenuItem> menuitems = new List<MenuItem>();
                    foreach(GameVersion version in game.Versions)
                    {
                        MenuItem menuItem = new MenuItem();
                        menuItem.Tag = version.ExePath;
                        menuItem.Header = version.Version;
                        menuItem.Click += MenuItem_Click;
                        menuitems.Add(menuItem);
                    }
                    miPlay.ItemsSource = menuitems;
                }
            }
        }
        #endregion

        #region Banner Play Version Click
        //The launcher needs to move to a seperate class. 
        //We need to track time spent in game and game status
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obMenuItem = e.OriginalSource as MenuItem;

            //
            LaunchExeProcess(obMenuItem.Tag.ToString());
        }

        //Move to another class
        private async void LaunchExeProcess(string executable)
        {
            await Task.Run(() =>
            {
                Process proc = new Process();
                proc.EnableRaisingEvents = true;
                proc.StartInfo.UseShellExecute = true;

                //proc.Exited += EclipseInstanceClosed;
                //proc.Disposed += EclipseInstanceDisposed;

                proc.StartInfo.FileName = executable;

                proc.Start();
            });
        }
        #endregion

        #region Windwow Buttons
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
            System.Windows.Application.Current.Shutdown();
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
                this.DragMove();
        }
        #endregion

        private void OnListBoxNavButtonUp(object sender, RoutedEventArgs e)
        {
            var Item = (ListBoxItem)sender;
            Console.WriteLine(Item.Name);
            if (Item.Name.ToString() == "Import")
            {
                BatchImporter batchImporter = new BatchImporter();
                //Set event handler to start parsing games once 
                batchImporter.btn_import.Click += Btn_Import_Click;
                batchImporter.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                batchImporter.Show();
            }
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            //This will take each game detail that was imported and change it into an actual game.
            //We have to take this list and put all version in a seperate class. Once this is complete we add to database.
            //after adding to database we can import to the BannerView
            foreach (var GameDetail in GameScanner.GameDetailList)
            {
                //We need to insert in to database first then get the id of the new item
                string recordID = Database.AddGame(GameDetail);
                if (recordID != string.Empty)
                {
                    Database.AddVersion(GameDetail, Convert.ToInt32(recordID));
                    bool GameExist = GameList.Any(x => GameDetail.Creator == x.Creator && GameDetail.Title == x.Title);
                    if (GameExist)
                    {
                        GameList.First(
                            item => item.Title == GameDetail.Title && item.Creator == GameDetail.Creator
                            ).Versions.Add(new GameVersion
                            {
                                Version = GameDetail.Version,
                                DateAdded = DateTime.Now,
                                ExePath = System.IO.Path.Combine(GameDetail.Folder, GameDetail.Executable[0]),
                                GamePath = GameDetail.Folder,
                                RecordId = Convert.ToInt32(recordID)
                            });

                    }
                    else
                    {
                        List<GameVersion> gameVersions = new List<GameVersion>();
                        gameVersions.Add(new GameVersion
                        {
                            Version = GameDetail.Version,
                            DateAdded = DateTime.Now,
                            ExePath = System.IO.Path.Combine(GameDetail.Folder, GameDetail.Executable[0]),
                            GamePath = GameDetail.Folder,
                            RecordId = Convert.ToInt32(recordID)
                        });

                        GameList.Add(new Game
                        {
                            Creator = GameDetail.Creator,
                            Title = GameDetail.Title,
                            Versions = gameVersions,
                            Engine = GameDetail.Engine,
                            Status = "",
                            ImageData = LoadImage("")
                        });
                    }
                }
                else
                {
                    Logging.Logger.Info(GameDetail.Title);
                }
                
                BannerView.Items.Refresh();
                GameListBox.Items.Refresh();
            }

            //sort items in lists
            GameListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Title",System.ComponentModel.ListSortDirection.Ascending));

        }


        //This needs to move to its own class. We need an image handler class
        private BitmapImage LoadImage(string path)
        {
            var image = new BitmapImage(new Uri("pack://application:,,,/Assets/Images/default.jpg"));
            if (path == "")
            {
                return image;
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
                    return image;
                }
            }
        }
    }
}