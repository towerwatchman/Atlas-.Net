using Atlas.Core;
using Atlas.Core.Database;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Atlas.UI.Windows
{
    /// <summary>
    /// Interaction logic for BatchImporter.xaml
    /// </summary>

    public delegate void StartImportEventHandler(object sender, EventArgs e);

    public partial class BatchImporter : Window
    {
        #region Event Handler for Sending Import Command to MainWindow
        public event StartImportEventHandler StartImport;

        public void EmitImportSignal()
        {
            if (StartImport != null)
            {
                StartImport(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Importers
        Pages.Importers.Custom customImporter = new Pages.Importers.Custom();
        Pages.Importers.Storefront steamImporter = new Pages.Importers.Storefront();

        #endregion

        //private 
        public BatchImporter()
        {
            InitializeComponent();
            //Assign UI elements to InterfaceHelper
            InterfaceHelper.GameScannerProgressBar = pbGameScanner;
            InterfaceHelper.PotentialGamesTextBox = tbPotentialGames;
            InterfaceHelper.ImporterScanTextBox = ImporterScanTextBox;

            //Disable Import,Next Button & progressbar
            //btn_next.IsEnabled = false;
            btn_import.IsEnabled = true;
            pbGameScanner.Visibility = Visibility.Hidden;

            //Add Scanners to list

            ImportSourceComboBox.SelectedIndex = 0;
            ImportFrameNavigation.Content = customImporter;
            //cbScanner.Items.Add("Steam");
            //tb_format.Text = Settings.Config.FolderStructure;
        }
        #region Windows Buttons
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                this.BorderThickness = new System.Windows.Thickness(4);
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
        #endregion


        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btn_next_Click(object sender, RoutedEventArgs e)
        {
            var item = (TabItem)tbc_Import.SelectedItem;

            Atlas.Core.Global.DeleteAfterImport = (bool)customImporter.cb_DeleteFolder.IsChecked;

            if (item.Header.ToString() == "Start")
            {
                //VN and H Games. F95 Mostly
                if (ImportSourceComboBox.SelectedIndex == 0)
                {
                    await f95ImportAsync();
                    if (customImporter.cb_compression.IsChecked == true)
                    {
                        btn_matches.Visibility = Visibility.Visible;
                        btn_matches.Width = 70;
                    }
                }
            }

        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            //Before we can start the import we need to verify a few details
            //Check that the Creator column is valid
            if (F95Scanner._GameDetailList.Count > 0)
            {
                EmitImportSignal();
                this.Close();

                /* if (F95Scanner._GameDetailList.Any(x=>x.Creator != string.Empty) &&
                     F95Scanner._GameDetailList.Any(x => x.Title != string.Empty)
                 )
                 {                   
                 }*/
            }

        }

        private void ImportSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var ComboBox = sender as ComboBox;
            if (ComboBox != null && ImportFrameNavigation != null)
            {
                if (ComboBox.SelectedIndex == 0)
                {
                    ImportFrameNavigation.Content = customImporter;
                }
                if (ComboBox.SelectedIndex == 1)
                {
                    ImportFrameNavigation.Content = steamImporter;
                }
            }
        }

        #region Importers

        private async Task f95ImportAsync()
        {
            if (Directory.Exists(customImporter.tb_FolderDialog.Text))
            {
                tbc_Import.SelectedIndex = 1;
                //GameScanner gameScanner = new GameScanner();
                //GameList.ItemsSource = F95Scanner.GameDetailList;
                InterfaceHelper.Datagrid = GameList;

                string folder = customImporter.tb_FolderDialog.Text;

                string format = (bool)customImporter.cb_format.IsChecked == true ? "" : customImporter.tb_format.Text;

                //Hide Next Button
                btn_next.Width = 0;
                btn_next.Visibility = Visibility.Hidden;
                //Show Import Button
                btn_import.Visibility = Visibility.Visible;
                btn_import.IsEnabled = false;
                btn_import.Width = 70;

                //gets extension from textbox
                string[] extensions = customImporter.OtherDefaultExt.Text.Split(",");

                bool isArchive = (bool)customImporter.cb_compression.IsChecked;

                await Task.Run(async () =>
                {
                    await F95Scanner.Start(folder, format, extensions, isArchive);
                });

                btn_import.IsEnabled = true;
            }
        }

        #endregion

        //go through datatable and check each item. if there data has changed then search database for match
        private void btn_Matches_Click(object sender, RoutedEventArgs e)
        {
            int index = 0;

            var tempGameDetailsList = F95Scanner._GameDetailList.ToList();
            foreach(GameDetails game in tempGameDetailsList)
            {
                var data = SQLiteInterface.GetAtlasId(game.Title, game.Creator);
                List<string> results = new List<string>();

                if (data.Count == 1)
                {
                    game.Id = data[0][0];
                    game.Title = data[0][1];
                    game.Creator = data[0][2];
                    game.Engine = data[0][3];
                }
                else
                {
                    if (data.Count > 1)
                    {
                        foreach (var item in data)
                        {
                            results.Add($"{item[0]} | {item[1]} | {item[2]}");
                        }
                    }
                }

                F95Scanner._GameDetailList[index] = game;
                GameList.Items.Refresh();

                index++;
            }
        }
    }
}
