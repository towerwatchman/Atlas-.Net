using Atlas.Core;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Atlas.UI.Importer
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
            if(StartImport != null)
            {
                StartImport(this, EventArgs.Empty);
            }
        }
        #endregion

        public BatchImporter()
        {
            InitializeComponent();
            //Assign UI elements to InterfaceHelper
            InterfaceHelper.GameScannerProgressBar = pbGameScanner;
            InterfaceHelper.PotentialGamesTextBox = tbPotentialGames;
            InterfaceHelper.ImporterScanTextBox = ImporterScanTextBox;

            //Disable Import,Next Button & progressbar
            btn_next.IsEnabled = false;
            btn_import.IsEnabled = true;
            pbGameScanner.Visibility = Visibility.Hidden;

            //Add Scanners to list
            cbScanner.Items.Add("Atlas Scraper");
            //cbScanner.Items.Add("Steam");


            tb_format.Text = Settings.Config.FolderStructure;
        }
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

        private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            var result = dialog.ShowDialog();

            var folder = dialog.FolderName;
            if (folder != null)
            {
                tb_FolderDialog.Text = folder;
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btn_next_Click(object sender, RoutedEventArgs e)
        {
            var item = (TabItem)tbc_Import.SelectedItem;

            if (item.Header.ToString() == "Start")
            {
                tbc_Import.SelectedIndex = 1;
                //GameScanner gameScanner = new GameScanner();
                //GameList.ItemsSource = F95Scanner.GameDetailList;
                InterfaceHelper.Datagrid = GameList;

                string folder = tb_FolderDialog.Text;

                string format = (bool)cb_format.IsChecked == true ? "" : tb_format.Text;

                //Hide Next Button
                btn_next.Width = 0;
                btn_next.Visibility = Visibility.Hidden;
                //Show Import Button
                btn_import.Visibility = Visibility.Visible;
                btn_import.IsEnabled = false;
                btn_import.Width = 70;

                await Task.Run(async () =>
                {
                    await F95Scanner.Start(folder, format);
                });

                btn_import.IsEnabled = true;
            }

        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            //Before we can start the import we need to verify a few details
            //Check that the Creator column is valid
            if(F95Scanner._GameDetailList.Count > 0)
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

        private void cb_format_Click(object sender, RoutedEventArgs e)
        {
            tb_format.IsEnabled = !(bool)cb_format.IsChecked;
        }

        private void tb_FolderDialog_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Check if the current text in the Root Path is a valid folder
            btn_next.IsEnabled = Directory.Exists(tb_FolderDialog.Text);
        }
    }
}
