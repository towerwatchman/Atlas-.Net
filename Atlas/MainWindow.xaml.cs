using Atlas.Core;
using Atlas.Core.Database;
using Atlas.Core.Networking;
using Atlas.Core.Utilities;
using Atlas.UI;
using Atlas.UI.Pages;
using Atlas.UI.ViewModel;
using Atlas.UI.Windows;
using NLog;
using SixLabors.ImageSharp.Formats.Tga;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Networking.NetworkOperators;
//using System.Windows.Shapes;

namespace Atlas
{
    public partial class MainWindow : Window
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ObservableCollection<NotificationViewModel> NotificationCollection = new ObservableCollection<NotificationViewModel>();
        private bool isBatchImporterOpen = false;
        private bool isRefreshRunning = false;
        private bool isGameDetailShown = false;

        //Create private members for each page
        BannerViewPage bvp = new BannerViewPage();
        NotificationsPage notificationsPage = new NotificationsPage();
        private SettingsWindow settingsWindow = new SettingsWindow();
        public MainWindow()
        {
            InitializeComponent();
            atlas_frame.Content = bvp;
            Atlas.Core.Global.DeleteAfterImport = false;

            GameImportBox.Visibility = Visibility.Hidden;

            InterfaceHelper.MainWindow = this;

            //Assign Update Section to InterfaceHelper
            InterfaceHelper.GameImportBox = GameImportBox;
            InterfaceHelper.GameImportTextBox = GameImportTextBox;
            InterfaceHelper.GameImportPB = GameImportPB;
            InterfaceHelper.GameImportPBStatus = GameImportPBStatus;


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
            InterfaceHelper.NotificationsPage = notificationsPage.NotificationsList;

            //Initalize the BannerViews
            //InitListView();
            InitBannerView();

            //Assign Left click event to BannerView
            bvp.BannerView.MouseLeftButtonUp += BannerView_MouseLeftButtonUp;

            //Hide clear text
            ClearSearchBox.Visibility = Visibility.Hidden;

            //Set First time setup as complete
            Atlas.Core.Settings.Config.FTS = true;

            ModelData.NotificationCollection = NotificationCollection;
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                notificationsPage.NotificationsList.Items.Clear();
                notificationsPage.NotificationsList.ItemsSource = null;
                notificationsPage.NotificationsList.ItemsSource = ModelData.NotificationCollection;
                notificationsPage.NotificationsList.Items.Refresh();

            }));
        }

        private void BannerView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.atlas_frame.Content = new GameDetailPage((GameViewModel)bvp.BannerView.SelectedItem);
        }

        private void InitBannerView()
        {
            Game game = new Game
            {
                AtlasID = -1,
                RecordID = -1,
                F95ID = -1,
                Title = "",
                Creator = "",
                Engine = "",
                Versions = new List<GameVersion>()
            };
            if (ModelData.GameCollection.Count <= 0)
            {
                ModelData.GameCollection.Add(new GameViewModel(game));
            }

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                this.GameListBox.Items.Clear();
                this.GameListBox.ItemsSource = null;
                this.GameListBox.ItemsSource = ModelData.GameCollection;
                this.GameListBox.Items.Refresh();
                bvp.BannerView.Items.Clear();
                bvp.BannerView.ItemsSource = null;
                bvp.BannerView.ItemsSource = ModelData.GameCollection;
                bvp.BannerView.Items.Refresh();

                //Bind notifications
                notificationsPage.NotificationsList.Items.Clear();
                notificationsPage.NotificationsList.ItemsSource = null;
                notificationsPage.NotificationsList.ItemsSource = ModelData.NotificationCollection;
                notificationsPage.NotificationsList.Items.Refresh();

            }));

            try
            {
                //if (bvp.BannerView.Items.Count > 0)
                //{
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(bvp.BannerView.ItemsSource);
                view.Filter = UserFilter;
                //}
                // the code that's accessing UI properties
                //sort items in lists
                GameListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Title", System.ComponentModel.ListSortDirection.Ascending));

                //Set total Versions & games
                TotalGames.Text = $"{ModelData.TotalGames} Games Installed, {ModelData.TotalVersions} Total Versions";
            }
            catch (Exception ex) { Logger.Error(ex); }
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
                    //Clear all menu items
                    bvp.cmGame.Items.Clear();

                    MenuItem playItem = new MenuItem();
                    playItem.FontSize = 14;
                    playItem.FontWeight = FontWeights.Bold;
                    //playItem.Style = FindResource("PlayMenuItem") as Style;
                    playItem.Header = "Play";

                    /*                    <MenuItem x:Name="miPlay"
                              Header="Play"
                              FontSize="14"
                              FontWeight="Bold"
                              HorizontalContentAlignment="Center" Style="{DynamicResource PlayMenuItem}">
                        <MenuItem.Background>
                            <LinearGradientBrush EndPoint="0.5,1"
                                                 StartPoint="0.5,0">
                                <GradientStop Color="#72CF00" />
                                <GradientStop Color="#56B628"
                                              Offset="0" />
                            </LinearGradientBrush>
                        </MenuItem.Background>
                        <MenuItem x:Name="miVersions"
                                  Header="Versions" />
                    </MenuItem>
                    <MenuItem Header="Add to favorites" />
                    <MenuItem Header="Manage">
                        <MenuItem Header="Add desktop icon" />
                        <MenuItem Header="Browse local files" />
                        <MenuItem Header="Set custom artwork" />
                    </MenuItem>*/

                    //bvp.miPlay.ItemsSource = null;
                    //bvp.miPlay.Items.Clear();

                    List<MenuItem> menuitems = new List<MenuItem>();
                    if (game.Versions != null)
                    {
                        foreach (GameVersion version in game.Versions)
                        {
                            MenuItem menuItem = new MenuItem();
                            menuItem.Tag = version.ExePath;
                            menuItem.Header = version.Version;
                            menuItem.Click += PlayVersion_Click;
                            menuitems.Add(menuItem);
                        }
                    }
                    playItem.ItemsSource = menuitems;
                    bvp.cmGame.Items.Add(playItem);

                    //Open Intall Directory
                    MenuItem IntallItem = new MenuItem();
                    IntallItem.Tag = game.Versions[0].GamePath.ToString();
                    IntallItem.Header = "Open Game Folder";
                    IntallItem.Click += InstallLocation_Click;
                    bvp.cmGame.Items.Add(IntallItem);

                    //Open Intall Directory
                    MenuItem urlItem = new MenuItem();
                    urlItem.Tag = game.SiteUrl.ToString();
                    urlItem.Header = "Open Web Link";
                    urlItem.Click += Url_Click;
                    bvp.cmGame.Items.Add(urlItem);


                    //Properties
                    MenuItem PropertyItem = new MenuItem();
                    PropertyItem.Tag = game.RecordID.ToString();
                    PropertyItem.Header = "Properties";
                    PropertyItem.Click += GameProperties_Click;
                    bvp.cmGame.Items.Add(PropertyItem);
                }
            }
        }

        private void Url_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obMenuItem = e.OriginalSource as MenuItem;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = obMenuItem.Tag.ToString(),
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void InstallLocation_Click(object sender, RoutedEventArgs e)
        {
            MenuItem obMenuItem = e.OriginalSource as MenuItem;

            LaunchExeProcess(obMenuItem.Tag.ToString());
        }
        #endregion

        #region Banner Play Version Click
        //The launcher needs to move to a seperate class. 
        //We need to track time spent in game and game status
        private void PlayVersion_Click(object sender, RoutedEventArgs e)
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

        private void OnListBoxNavButtonUp(object sender, RoutedEventArgs e)
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
                batchImporter.StartImport += BatchImporter_StartImport;
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
                    Logger.Info("Running metadata refresh for all game objects");
                    //Disable List box
                    isRefreshRunning = true;
                    //sort list by title
                    var tempList = ModelData.GameCollection.OrderBy(o => o.Title);
                    //download images
                    _ = Task.Run(() =>
                    {
                        foreach (GameViewModel game in tempList)
                        {
                            try
                            {
                                //Get Banner Path for database. This will be relative and the root path will need to be added. 
                                string banner_relative_path = SQLiteInterface.GetBannerPath(game.RecordID.ToString());
                                string banner_full_path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, banner_relative_path);
                                //Check if banner already exist
                                if ((banner_relative_path == string.Empty && game.AtlasID > -1) || !File.Exists(banner_full_path))
                                {
                                    //Run below in a new task that is awaited
                                    Task.Run(async () =>
                                    {
                                        string bannerUrl = SQLiteInterface.GetBannerUrl(game.AtlasID.ToString());
                                        if (bannerUrl != string.Empty)
                                        {
                                            Logger.Info($"Downloading images id:{game.RecordID} name:{game.Title}");
                                            Directory.CreateDirectory(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\images", game.RecordID.ToString()));

                                            banner_full_path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\images", game.RecordID.ToString(), Path.GetFileNameWithoutExtension(bannerUrl));
                                            banner_relative_path = Path.Combine("data\\images", game.RecordID.ToString(), Path.GetFileNameWithoutExtension(bannerUrl));

                                            try
                                            {
                                                //Download base image in bytes we will be
                                                byte[] imageBytes = await NetworkHelper.DownloadImageBytesAsync(bannerUrl);
                                                //Small Capsule
                                                bool isScCreated = await ImageInterface.ConvertToWebpAsync(imageBytes, 90, 600, $"{banner_full_path}_sc.webp");

                                                //Main Capsule
                                                bool isMcCreater = await ImageInterface.ConvertToWebpAsync(imageBytes, 90, 1260, $"{banner_full_path}_mc.webp");

                                                if (isScCreated && isScCreated)
                                                {
                                                    //update banner table
                                                    if (File.Exists($"{banner_full_path}_sc.webp"))
                                                    {
                                                        SQLiteInterface.UpdateBanners(game.RecordID, $"{banner_relative_path}_sc.webp", "banner");

                                                        Logger.Info($" Updated Banner Images for: {game.Title.ToString()}");
                                                        //Find Game in gamelist and set the banner to it
                                                        BitmapSource img = await ImageInterface.LoadImageAsync(game.RecordID,
                                                                    bannerUrl == "" ? "" : $"{banner_full_path}_sc.webp",
                                                                    Atlas.Core.Settings.Config.ImageRenderWidth,
                                                                    Atlas.Core.Settings.Config.ImageRenderHeight);
                                                        //Freeze Image so it can update main UI thread
                                                        img.Freeze();

                                                        GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == game.RecordID).FirstOrDefault();
                                                        var index = ModelData.GameCollection.IndexOf(gameObj);

                                                        if (gameObj != null)
                                                        {
                                                            ModelData.GameCollection[index].BannerImage = img;
                                                            ModelData.GameCollection[index].SmallCapsule = $"{banner_full_path}_sc.webp";
                                                            ModelData.GameCollection[index].MainCapsule = $"{banner_full_path}_mc.webp";
                                                            gameObj.OnPropertyChanged("BannerImage");
                                                        }
                                                    }
                                                }


                                            }
                                            catch (Exception ex) { Logger.Error(ex); }
                                        }
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();

                                    });
                                    //Set a default waiting period for downloading images between 200ms and 1s
                                    Random random = new Random();
                                    System.Threading.Thread.Sleep(random.Next(1000, 2000));
                                }

                                string[] screens = SQLiteInterface.getScreensUrlList(game.RecordID.ToString());
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.ToString());
                            }
                        }
                    });
                    isRefreshRunning = false;
                    Logger.Info("Game metadata refresh complete");
                }
                else
                {
                    Logger.Warn("Game metadata refresh is already running");
                }
                Home.IsSelected = true;
            }
            if (Item.Name.ToString() == "Settings")
            {
                //Check if settings window is null from being closed
                if (settingsWindow == null || settingsWindow.IsLoaded == false)
                {
                    settingsWindow = new SettingsWindow();
                    settingsWindow.Closed += SettingsWindow_Closed;
                }
                if (!settingsWindow.IsVisible)
                {
                    settingsWindow.Show();
                }
            }
        }

        private void SettingsWindow_Closed(object sender, EventArgs e)
        {
            Home.IsSelected = true;
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
            //Show Game Import Box
            GameImportBox.Visibility = Visibility.Visible;

            //await InitBannerView();
            //List<Game> = ModelData.Games;
            //This will take each game detail that was imported and change it into an actual game.
            //We have to take this list and put all version in a seperate class. Once this is complete we add to database.
            //after adding to database we can import to the BannerView

            await Task.Run(async () =>
            {

                foreach (var GameDetail in F95Scanner.GameDetailList)
                {

                    //This is to test that we can make notifications
                    Notification notification = new Notification();
                    notification.Title = GameDetail.Title;
                    notification.Version = GameDetail.Version;
                    notification.ProgressBarValue = 40;
                    notification.Status = "Running Update";
                    notification.Id = Notifications.currentId != 0 ? Notifications.currentId++ : 1;

                    // Logger.Info(GameDetail.Title);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NotificationCollection.Add(new NotificationViewModel(notification));
                        GameImportTextBox.Content = $"Adding Game: {GameDetail.Title} | Version: {GameDetail.Version}";
                    });
                    //In some instances the database data will have spaces at the end. We need to remove those. 
                    GameDetail.Creator = GameDetail.Creator.Trim();
                    GameDetail.Title = GameDetail.Title.Trim();
                    GameDetail.Version = GameDetail.Version.Trim();

                    string[] archiveExt = Atlas.Core.Settings.Config.ExtractionExt.Split(',');
                    //Check if the file is an archive. 
                    if (archiveExt.Length > 0 && archiveExt.Any(GameDetail.Executable[0].Contains))
                    {
                        string input = "";
                        if (Directory.Exists(GameDetail.Folder))
                        {
                            input = Path.Combine(GameDetail.Folder, GameDetail.Executable[0]);
                        }
                        else
                        {
                            input = GameDetail.Folder;
                        }

                        string output = Atlas.Core.Settings.Config.GamesPath;
                        Logger.Warn(input);
                        if (GameDetail.Creator != string.Empty)
                        {
                            string creator = string.Join(" ", GameDetail.Creator.Replace("\\", "&").Replace("/", "&").Split(Path.GetInvalidFileNameChars()));
                            string title = string.Join(" ", GameDetail.Title.Split(Path.GetInvalidFileNameChars()));
                            string version = string.Join(" ", GameDetail.Version.Split(Path.GetInvalidFileNameChars()));

                            output += $"\\{creator}\\{title}\\{version}";
                            if (!Directory.Exists(output)) { Directory.CreateDirectory(output); }
                            bool extStatus = Compression.ExtractFile(input, output, GameDetail.Title, notification.Id);

                            if (extStatus == false)
                            {
                                Logger.Error($"Error extracting file {input}");
                                break;
                            }
                            //Make sure to change gane Path
                            else
                            {
                                GameDetail.Folder = output;
                            }

                            //Delete file if enabled
                            if (Atlas.Core.Global.DeleteAfterImport && extStatus == true)
                            {
                                try
                                {
                                    File.Delete(input);
                                }
                                catch (Exception ex) { Logger.Error(ex); }
                            }

                        }

                        //We now need to find the executable. 

                        //F95Scanner.FindGame(file, "", Atlas.Core.Settings.Config.ExecutableExt.Split(","), "", 1, 0, true);
                        List<string> executables = Executable.DetectExecutable(Directory.GetFiles(output), Atlas.Core.Settings.Config.ExecutableExt.Split(","));
                        GameDetail.Executable = executables;
                        if (executables.Count <= 0)
                        {
                            GameDetail.SingleExecutable = "";
                        }
                        if (executables.Count == 1)
                        {
                            GameDetail.SingleExecutable = executables[0];
                        }

                    }

                    await Task.Run(async () =>
                   {
                       //We need to insert in to database first then get the id of the new item
                       GameDetail.RecordID = SQLiteInterface.FindRecordID(GameDetail.Title, GameDetail.Creator).ToString();

                       if (GameDetail.RecordID == "-1")
                       {
                           GameDetail.RecordID = SQLiteInterface.AddGame(GameDetail);
                           //Logger.Info($"Adding game: {GameDetail.Title}");
                       }

                       if (GameDetail.RecordID != string.Empty)
                       {
                           if (GameDetail.RecordID != "" && GameDetail.AtlasID != "")
                           {
                               SQLiteInterface.SetAtlasMapping(GameDetail.RecordID, GameDetail.AtlasID);
                           }

                           if (SQLiteInterface.CheckIfVersionExist(GameDetail.RecordID, GameDetail.Version) == false)
                           {
                               //Check if there is an executable
                               if (GameDetail.Executable.Count == 0)
                               {
                                   Logger.Warn($"No valid executables were found for Game: {GameDetail.Title} | Version: {GameDetail.Version}");
                               }

                               SQLiteInterface.AddVersion(GameDetail, Convert.ToInt32(GameDetail.RecordID));
                           }
                           //Check if we need to download images before adding the gamemodel
                           if (Atlas.Core.Settings.DownloadImages)
                           {
                               await ImageInterface.DownloadImages(GameDetail);
                           }

                           Logger.Info($"Adding Entry for: {GameDetail.Title} | Version: {GameDetail.Version}");


                           //Make sure there is only one instance of each game type based on title and creator.

                           Application.Current.Dispatcher.Invoke(() =>
                           {
                               if (!ModelData.GameCollection.Where(x => x.RecordID == Convert.ToInt32(GameDetail.RecordID)).Any())
                               {
                                   ModelData.GameCollection.Add(new GameViewModel(SQLiteInterface.RetrieveGame(GameDetail.RecordID).Result));
                               }
                               //Update the model with new data if it already exist
                               else
                               {
                                   GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == Convert.ToInt32(GameDetail.RecordID)).FirstOrDefault();
                                   var index = ModelData.GameCollection.IndexOf(gameObj);

                                   if (gameObj != null)
                                   {
                                       ModelData.GameCollection[index] = new GameViewModel(SQLiteInterface.RetrieveGame(GameDetail.RecordID).Result);
                                       bvp.BannerView.Items.Refresh();
                                   }
                               }

                               //Update game data counts
                               ModelData.TotalGames = ModelData.GameCollection.Count;
                               int versions = 0;
                               foreach (var modelData in ModelData.GameCollection)
                               {
                                   versions += modelData.Versions.Count;
                               }
                               ModelData.TotalVersions = versions;
                               TotalGames.Text = $"{ModelData.TotalGames} Games Installed, {ModelData.TotalVersions} Total Versions";
                           });
                       }
                       else
                       {
                           Logger.Warn($"Unable to add Game: {GameDetail.Title}");
                       }

                       GC.WaitForPendingFinalizers();
                       GC.Collect();
                   });

                }
            });

            GameImportBox.Visibility = Visibility.Hidden;
        }

        private void AtlasSearchBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (AtlasSearchBox.Text == "Search Atlas")
            {
                AtlasSearchBox.Text = string.Empty;
            }
            //SearchBarBorder.BorderThickness = new Thickness(1);
            SearchBarBorder.SetResourceReference(Control.BorderBrushProperty, "WindowAccent");
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
            //Logger.Warn(AtlasSearchBox.Text.ToString());
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

            /*ContextMenu contextMenu = new ContextMenu();
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
            AddGameButton.ContextMenu.IsOpen = true;*/
            isBatchImporterOpen = true;
            BatchImporter batchImporter = new BatchImporter();
            //Set event handler to start parsing games once 
            batchImporter.StartImport += BatchImporter_StartImport; ;
            batchImporter.Closed += BatchImporter_Closed;

            batchImporter.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            batchImporter.Show();

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
        private void OpenNotifications_Click(object sender, RoutedEventArgs e)
        {
            atlas_frame.Content = notificationsPage;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //check for min or max.
            if (WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new System.Windows.Thickness(7);
            }
        }

        private void GameProperties_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;


            string RecordID = menuItem.Tag.ToString();

            GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == Convert.ToInt32(RecordID)).FirstOrDefault();

            if (!isGameDetailShown)
            {
                GameDetailWindow gameDetailWindow = new GameDetailWindow(gameObj);
                gameDetailWindow.Show();
            }
        }

    }
}