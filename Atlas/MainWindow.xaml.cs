﻿using Atlas.Core;
using Atlas.Core.Database;
using Atlas.UI.Importer;
using Atlas.Core.Network;
using Castle.Core.Resource;
using NLog;
using SharpVectors.Dom.Css;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;
using Atlas.UI;
using System.Windows.Threading;
using System.Net.Cache;
using System.Drawing;
using System.Windows.Interop;
using System.Drawing.Imaging;
using Atlas.Core.Utilities;
using System.Windows.Data;

namespace Atlas
{
    public partial class MainWindow : Window
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool isBatchImporterOpen = false;
        private bool isRefreshRunning = false;
        private List<Game> GameList = new List<Game>();
        public MainWindow()
        {
            InitializeComponent();

            
            //Custom Event manager for pressing one of the Navigation buttons on the left side
            EventManager.RegisterClassHandler(typeof(ListBoxItem), ListBoxItem.MouseLeftButtonUpEvent, new RoutedEventHandler(this.OnListBoxNavButtonUp));

            //Check if we need to show the listview
            if (Atlas.Core.Settings.Config.ShowListView == false)
            {
                HideListView();
            }

            //Hide context menu
            cmGame.Visibility = Visibility.Hidden;
            BannerView.MouseUp += BannerView_MouseUp;

            //Assign version
            tbVersion.Text = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

            InterfaceHelper.BannerView = BannerView;

            //initalize the BannerView
            InitListView();
            //LoadGames();
            
        }

        private void InitListView()
        {
            //Reset the list
            this.GameListBox.ItemsSource = null;
            this.BannerView.ItemsSource = ModelData.Games;
            this.GameListBox.ItemsSource = ModelData.Games;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(BannerView.ItemsSource);
            view.Filter = UserFilter;
            // the code that's accessing UI properties
            //sort items in lists
            GameListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Title", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void LoadGames()
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke(async () =>
                {
                    //Build Default Games List
                    //await SQLiteInterface.BuildGameListAsync(GameList);

                    //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(BannerView.ItemsSource);
                    //view.Filter = UserFilter;
                    // the code that's accessing UI properties
                    //sort items in lists
                    //GameListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Title", System.ComponentModel.ListSortDirection.Ascending));

                    //update list to show images and data
                });
            });
        }

        #region Banner Left Click
        private void BannerView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (this.BannerView.ItemsSource != null && this.BannerView.Items.Count > 0)
            {

                Game game = BannerView.SelectedItem as Game;
                if (game != null)
                {
                    cmGame.Visibility = Visibility.Visible;
                    miPlay.ItemsSource = null;
                    miPlay.Items.Clear();

                    List<MenuItem> menuitems = new List<MenuItem>();
                    if (game.Versions != null)
                    {
                        foreach (GameVersion version in game.Versions)
                        {
                            MenuItem menuItem = new MenuItem();
                            menuItem.Tag = version.ExePath;
                            menuItem.Header = version.Version;
                            menuItem.Click += MenuItem_Click;
                            menuitems.Add(menuItem);
                        }
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

        private async void OnListBoxNavButtonUp(object sender, RoutedEventArgs e)
        {
            var Item = (ListBoxItem)sender;
            Console.WriteLine(Item.Name);
            if (Item.Name.ToString() == "Import" && isBatchImporterOpen == false)
            {
                isBatchImporterOpen = true;
                BatchImporter batchImporter = new BatchImporter();
                //Set event handler to start parsing games once 
                batchImporter.StartImport += BatchImporter_StartImport; ;
                batchImporter.Closed += BatchImporter_Closed;

                batchImporter.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                batchImporter.Show();
            }

            if (Item.Name.ToString() == "ShowList")
            {
                if (GameListBox.Visibility == Visibility.Visible)
                {
                    HideListView();
                    Atlas.Core.Settings.Config.ShowListView = false;
                }
                else
                {
                    ShowListView();
                    Atlas.Core.Settings.Config.ShowListView = true;
                }
            }

            if (Item.Name.ToString() == "Refresh")
            {
                //Keep user from pressing refresh more than once
                if (isRefreshRunning == false)
                {
                    //Disable List box

                    isRefreshRunning = true;
                    List<Game> tempList = ModelData.Games.OrderBy(o=> o.Title).ToList() ;
                    //download images
                    await Task.Run(async () =>
                        {
                            foreach (Game game in tempList)
                            {
                                try
                                {
                                    //check if banner already exist
                                    if (SQLiteInterface.GetBannerPath(game.RecordID.ToString()) == string.Empty && game.AtlasID != "")
                                    {
                                        string bannerUrl = SQLiteInterface.GetBannerUrl(game.AtlasID);

                                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data\\images", game.RecordID.ToString()));
                                        string banner_path = Path.Combine(Directory.GetCurrentDirectory(), "data\\images", game.RecordID.ToString(), Path.GetFileName(bannerUrl));
                                        Atlas.Core.Network.NetworkInterface networkInterface = new Core.Network.NetworkInterface();
                                        await Core.Network.NetworkInterface.DownloadFileAsync(bannerUrl, banner_path, 200);
                                        //update banner table
                                        if (bannerUrl != "")
                                        {
                                            SQLiteInterface.UpdateBanners(game.RecordID, banner_path, "banner");
                                        }
                                        //game.ImageData = LoadImage(banner_path);

                                        Logger.Info(game.Title.ToString());

                                        //Find Game in gamelist and set the banner to it
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            ModelData.Games[ModelData.Games.FindIndex(x => x.RecordID == game.RecordID)].ImageData =
                                            ImageInterface.LoadImage(
                                                    bannerUrl == "" ? "" : banner_path,
                                                    Atlas.Core.Settings.Config.ImageRenderWidth,
                                                    Atlas.Core.Settings.Config.ImageRenderHeight);

                                        //replace game item with new data

                                        
                                            this.BannerView.Items.Refresh();
                                        });
                                        //Hack to free up memory
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex.ToString());
                                }
                            }
                        });
                    isRefreshRunning = false;
                }
            }
        }

        #region Game ListView Visibility
        private void ShowListView()
        {
            //Show the list view
            GameListBox.Visibility = Visibility.Visible;
            //Change icon to list
            ShowList.Content = FindResource("list_icon");
            //Clear the current column definitions
            RecordView.ColumnDefinitions.Clear();
            //Reset the Column Definitions
            var c1 = new ColumnDefinition();
            var c2 = new ColumnDefinition();
            c1.Width = new GridLength(200, GridUnitType.Pixel);
            c2.Width = new GridLength(1, GridUnitType.Star);
            RecordView.ColumnDefinitions.Add(c1);
            RecordView.ColumnDefinitions.Add(c2);
            Home.IsSelected = true;
        }
        private void HideListView()
        {
            //Hide the list view
            GameListBox.Visibility = Visibility.Hidden;
            //Change the icon to a grid
            ShowList.Content = FindResource("grid_icon");
            //Clear the current column definitions
            RecordView.ColumnDefinitions.Clear();
            //Reset the Column Definitions
            var c1 = new ColumnDefinition();
            c1.Width = new GridLength(1, GridUnitType.Star);
            RecordView.ColumnDefinitions.Add(c1);
            Home.IsSelected = true;
        }
        #endregion

        private void BatchImporter_Closed(object sender, EventArgs e)
        {
            isBatchImporterOpen = false;
            Home.IsSelected = true;
        }

        private void BatchImporter_StartImport(object sender, EventArgs e)
        {
            isBatchImporterOpen = false;
            //This will take each game detail that was imported and change it into an actual game.
            //We have to take this list and put all version in a seperate class. Once this is complete we add to database.
            //after adding to database we can import to the BannerView


            foreach (var GameDetail in F95Scanner.GameDetailList)
            {
                //We need to insert in to database first then get the id of the new item
                string recordID = SQLiteInterface.FindRecordID(GameDetail.Title, GameDetail.Creator).ToString();

                if (recordID == "-1")
                {
                    recordID = SQLiteInterface.AddGame(GameDetail);
                }

                if (recordID != string.Empty)
                {
                    if (GameDetail.Id != "")
                    {
                        SQLiteInterface.SetAtlasMapping(recordID, GameDetail.Id);
                    }

                    if (SQLiteInterface.CheckIfVersionExist(recordID, GameDetail.Version) == false)
                    {
                        SQLiteInterface.AddVersion(GameDetail, Convert.ToInt32(recordID));
                    }
                    bool GameExist = GameList.Any(x => GameDetail.Creator == x.Creator && GameDetail.Title == x.Title);
                    //Check if there is already a version of the game in the database
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
                            ImageData = ImageInterface.LoadImage("", Atlas.Core.Settings.Config.ImageRenderWidth, Atlas.Core.Settings.Config.ImageRenderHeight),
                            RecordID = Convert.ToInt32(recordID),

                        });
                    }
                }
                else
                {
                    Logger.Info(GameDetail.Title);
                }


                //Download images


                //Once we are complete, do a full refresh
                InitListView();


            }

            //sort items in lists
            GameListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Title", System.ComponentModel.ListSortDirection.Ascending));

        }

        private void BannerView_CleanUpVirtualizedItem(object sender, CleanUpVirtualizedItemEventArgs e)
        {

        }

        private void AtlasSearchBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (AtlasSearchBox.Text == "Search Atlas")
            {
                AtlasSearchBox.Text = string.Empty;
            }
        }

        private void AtlasSearchBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (AtlasSearchBox.Text == string.Empty)
            {
                AtlasSearchBox.Text = "Search Atlas";
            }
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(AtlasSearchBox.Text) || AtlasSearchBox.Text == "Search Atlas")
                return true;
            else
                return ((item as Game).Title.IndexOf(AtlasSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void AtlasSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BannerView != null)
            {
                if (BannerView.ItemsSource != null)
                {
                    try
                    {
                        CollectionViewSource.GetDefaultView(BannerView.ItemsSource).Refresh();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
        }
    }
}