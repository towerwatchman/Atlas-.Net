using Atlas.Core;
using Atlas.Core.Database;
using Atlas.Core.Utilities;
using Atlas.UI;
using Atlas.UI.Pages;
using Atlas.UI.ViewModel;
using Atlas.UI.Windows;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Atlas
{
    public partial class MainWindow : Window
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool isBatchImporterOpen = false;
        private bool isRefreshRunning = false;

        //Create private members for each page
        BannerViewPage bvp = new BannerViewPage();
        private SettingsWindow settingsWindow = new SettingsWindow();
        public MainWindow()
        {
            InitializeComponent();
            atlas_frame.Content = bvp;


            //Custom Event manager for pressing one of the Navigation buttons on the left side
            EventManager.RegisterClassHandler(typeof(ListBoxItem), ListBoxItem.MouseLeftButtonUpEvent, new RoutedEventHandler(this.OnListBoxNavButtonUp));

            //Check if we need to show the listview
            if (Atlas.Core.Settings.Config.ShowListView == false)
            {
                HideListView();
            }

            //Hide context menu
            bvp.cmGame.Visibility = Visibility.Hidden;
            bvp.BannerView.MouseUp += BannerView_MouseUp;

            //Assign version
            tbVersion.Text = $"Version: {Assembly.GetExecutingAssembly().GetName().Version!.ToString()}";

            InterfaceHelper.BannerView = bvp.BannerView;

            //Initalize the BannerViews
            InitListView();

            //Assign Left click event to BannerView
            bvp.BannerView.MouseLeftButtonUp += BannerView_MouseLeftButtonUp;

            //Hide clear text
            ClearSearchBox.Visibility = Visibility.Hidden;

            //Set First time setup as complete
            Atlas.Core.Settings.Config.FTS = true;
        }

        private void BannerView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.atlas_frame.Content = new GameDetailPage((GameViewModel)bvp.BannerView.SelectedItem);
        }

        private void InitListView()
        {
            //Reset the list
            this.GameListBox.ItemsSource = null;
            bvp.BannerView.ItemsSource = null;
            bvp.BannerView.ItemsSource = ModelData.GameCollection;
            this.GameListBox.ItemsSource = ModelData.GameCollection;

            try
            {
                if (bvp.BannerView.Items.Count > 0)
                {
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(bvp.BannerView.ItemsSource);
                    view.Filter = UserFilter;
                }
                // the code that's accessing UI properties
                //sort items in lists
                GameListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Title", System.ComponentModel.ListSortDirection.Ascending));

                //Set total Versions & games
                TotalGames.Text = $"{ModelData.TotalGames} Games Installed, {ModelData.TotalVersions} Total Versions";
            }
            catch (Exception ex) { Logger.Error(ex); }
        }

        #region Banner Right Click
        private void BannerView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (bvp.BannerView.ItemsSource != null && bvp.BannerView.Items.Count > 0)
            {

                GameViewModel game = bvp.BannerView.SelectedItem as GameViewModel;
                if (game != null)
                {
                    bvp.cmGame.Visibility = Visibility.Visible;
                    bvp.miPlay.ItemsSource = null;
                    bvp.miPlay.Items.Clear();

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
                    bvp.miPlay.ItemsSource = menuitems;
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
                //this.AllowsTransparency = false;
                //WindowState = WindowState.Maximized;
                //this.BorderThickness = new System.Windows.Thickness(7);
                //WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Maximized;
                //WindowStyle = WindowStyle.None;
            }
            else
            {
                //this.AllowsTransparency = true;
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
                Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    this.DragMove();
                }));
        }
        #endregion

        private async void OnListBoxNavButtonUp(object sender, RoutedEventArgs e)
        {
            var Item = (ListBoxItem)sender;
            //Console.WriteLine(Item.Name);

            if (Item.Name.ToString() == "Home")
            {
                this.atlas_frame.Content = bvp;
            }

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
                    //sort list by title
                    var tempList = ModelData.GameCollection.OrderBy(o => o.Title);
                    //download images
                    await Task.Run(async () =>
                    {
                        foreach (GameViewModel game in tempList)
                        {
                            //Try to run as many as we can

                            try
                            {
                                //Get Banner Path for database
                                //Logger.Info(game.Title);
                                string banner_path = SQLiteInterface.GetBannerPath(game.RecordID.ToString());
                                //check if banner already exist
                                if ((banner_path == string.Empty && game.AtlasID > -1) || !File.Exists(banner_path))
                                {
                                    //Run below in a new task
                                    await Task.Run(async () =>
                                    {
                                        string bannerUrl = SQLiteInterface.GetBannerUrl(game.AtlasID.ToString());
                                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data\\images", game.RecordID.ToString()));
                                        byte[] ImageArray = null;

                                        /*banner_path = Path.Combine(Directory.GetCurrentDirectory(), "data\\images", game.RecordID.ToString(), Path.GetFileName(bannerUrl));
                                        await Core.Network.NetworkInterface.DownloadFileAsync(bannerUrl, banner_path, 200);*/

                                        if (Path.GetExtension(bannerUrl) == ".gif")
                                        {
                                            banner_path = Path.Combine(Directory.GetCurrentDirectory(), "data\\images", game.RecordID.ToString(), Path.GetFileName(bannerUrl));
                                            await Core.Network.NetworkInterface.DownloadFileAsync(bannerUrl, banner_path, 200);
                                        }
                                        else
                                        {
                                            banner_path = Path.Combine(Directory.GetCurrentDirectory(), "data\\images", game.RecordID.ToString(), Path.GetFileNameWithoutExtension(bannerUrl));
                                            ImageArray = await Core.Network.NetworkInterface.DownloadBytesAsync(bannerUrl, banner_path, 200);
                                        }

                                        //Atlas.Core.Network.NetworkInterface networkInterface = new Core.Network.NetworkInterface();


                                        if (ImageArray != null)
                                        {
                                            //ConvertImage to webp
                                            ImageInterface image = new ImageInterface();
                                            banner_path = image.ConvertToWebpAsync(ImageArray, banner_path, true).Result;

                                        }
                                        //update banner table
                                        if (File.Exists(banner_path))
                                        {
                                            SQLiteInterface.UpdateBanners(game.RecordID, banner_path, "banner");

                                            Logger.Info($" Updated Banner Images for: {game.Title.ToString()}");
                                            //Find Game in gamelist and set the banner to it
                                            BitmapSource img = ImageInterface.LoadImage(game.RecordID,
                                                        bannerUrl == "" ? "" : banner_path,
                                                        Atlas.Core.Settings.Config.ImageRenderWidth,
                                                        Atlas.Core.Settings.Config.ImageRenderHeight);
                                            //Freeze Image so it can update main UI thread
                                            img.Freeze();

                                            GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == game.RecordID).FirstOrDefault();
                                            var index = ModelData.GameCollection.IndexOf(gameObj);

                                            if (gameObj != null)
                                            {
                                                ModelData.GameCollection[index].BannerImage = img;
                                                ModelData.GameCollection[index].BannerPath = banner_path;
                                                gameObj.OnPropertyChanged("BannerImage");
                                            }
                                        }

                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                        //Set a default waiting period for downloading images
                                        System.Threading.Thread.Sleep(200);
                                    });
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
            if (Item.Name.ToString() == "Settings")
            {
                if (!settingsWindow.IsVisible)
                {
                    settingsWindow.Show();
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

        private async void BatchImporter_StartImport(object sender, EventArgs e)
        {

            isBatchImporterOpen = false;
            //List<Game> = ModelData.Games;
            //This will take each game detail that was imported and change it into an actual game.
            //We have to take this list and put all version in a seperate class. Once this is complete we add to database.
            //after adding to database we can import to the BannerView

            await Task.Run(async () =>
            {

                foreach (var GameDetail in F95Scanner.GameDetailList)
                {
                    await Task.Run(async () =>
                    {
                        //We need to insert in to database first then get the id of the new item
                        string recordID = SQLiteInterface.FindRecordID(GameDetail.Title, GameDetail.Creator).ToString();

                        if (recordID == "-1")
                        {
                            recordID = SQLiteInterface.AddGame(GameDetail);
                            Logger.Info($"Adding game: {GameDetail.Title}");
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
                            Logger.Info($"Adding version: {GameDetail.Version}");
                        }

                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    });

                }
            });

            /*await Application.Current.Dispatcher.Invoke(async () =>
            {
                // your code
                ModelLoader loader = new ModelLoader();
                await loader.CreateGamesList(Atlas.Core.Settings.Config.DefaultPage);
                InitListView();
            });*/
        }

        private void AtlasSearchBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (AtlasSearchBox.Text == "Search Atlas")
            {
                AtlasSearchBox.Text = string.Empty;
            }
            //SearchBarBorder.BorderThickness = new Thickness(1);
            SearchBarBorder.SetResourceReference(Control.BorderBrushProperty, "Accent");
        }

        private void AtlasSearchBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //check if text is entered or if the user has clicked on the checkbox
            if (AtlasSearchBox.Text == string.Empty && AtlasSearchBox.IsSelectionActive == false)
            {
                AtlasSearchBox.Text = "Search Atlas";
                SearchBarBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
                ClearSearchBox.Visibility = Visibility.Hidden;
            }
            //SearchBarBorder.BorderThickness = new Thickness(0);

        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(AtlasSearchBox.Text) || AtlasSearchBox.Text == "Search Atlas" || AtlasSearchBox.Text == "")
                return true;
            else
                return ((item as GameViewModel).Title.IndexOf(AtlasSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void AtlasSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Logger.Warn(AtlasSearchBox.Text.ToString());
            if (AtlasSearchBox.Text != "" && AtlasSearchBox.Text != "Search Atlas")
            {
                if (ClearSearchBox != null)
                {
                    if (AtlasSearchBox.Text != "" && AtlasSearchBox.Text != null && AtlasSearchBox.Text != "Search Atlas")
                    {
                        ClearSearchBox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ClearSearchBox.Visibility = Visibility.Hidden;
                    }
                }

                if (bvp.BannerView != null)
                {
                    if (bvp.BannerView.ItemsSource != null)
                    {
                        try
                        {
                            CollectionViewSource.GetDefaultView(bvp.BannerView.ItemsSource).Refresh();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }
        }

        private void GameListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Page currentPage = (Page)this.atlas_frame.Content;

            if (currentPage.Title == "BannerViewPage")
            {
                int index = GameListBox.SelectedIndex;
                bvp.BannerView.SelectedIndex = index;
                bvp.BannerView.ScrollIntoView(bvp.BannerView.SelectedItem);
            }
            else if (currentPage.Title == "GameDetailPage")
            {
                this.atlas_frame.Content = null;
                this.atlas_frame.Content = new GameDetailPage((GameViewModel)GameListBox.SelectedItem);
            }
        }

        private void AddGameButton_Click(object sender, RoutedEventArgs e)
        {

            //AddGameButton.ContextMenu.Items.Add("test");
            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();

            menuItem.Header = "Add Game";
            menuItem.Click += MenuItem_Click;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "Add Integration";
            menuItem.Click += MenuItem_Click;
            contextMenu.Items.Add(menuItem);

            AddGameButton.ContextMenu = contextMenu;
            //this will open for right and left click
            AddGameButton.ContextMenu.IsOpen = true;

        }

        private void ClearSearchBox_MouseEnter(object sender, MouseEventArgs e)
        {
            SearchBarBorder.BorderBrush = new SolidColorBrush(Colors.Transparent);
        }

        private void ClearSearchBox_Click(object sender, RoutedEventArgs e)
        {
            AtlasSearchBox.Text = "Search Atlas";
            ClearSearchBox.Visibility = Visibility.Hidden;
            CollectionViewSource.GetDefaultView(bvp.BannerView.ItemsSource).Refresh();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //check for min or max.
            if (WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new System.Windows.Thickness(7);
            }           
        }

        //Show context menu for adding games

    }
}