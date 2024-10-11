﻿using Atlas.Core;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
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

            //Disable Import,Next Button & progressbar
            btn_next.IsEnabled = false;
            btn_import.IsEnabled = false;
            pbGameScanner.Visibility = Visibility.Hidden;

            //Add Scanners to list
            cbScanner.Items.Add("F95");
            cbScanner.Items.Add("Steam");


            tb_format.Text = Settings.Config.FolderStructure;
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

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            var item = (TabItem)tbc_Import.SelectedItem;

            if (item.Header.ToString() == "Start")
            {
                tbc_Import.SelectedIndex = 1;
                //GameScanner gameScanner = new GameScanner();
                GameList.ItemsSource = F95Scanner.GameDetailList;
                InterfaceHelper.Datagrid = GameList;

                string folder = tb_FolderDialog.Text;

                string format = (bool)cb_format.IsChecked == true ? "" : tb_format.Text;

                Task.Factory.StartNew(() =>
                {
                    F95Scanner.Start(folder, format);
                });


                //Hide Next Button
                btn_next.Width = 0;
                btn_next.Visibility = Visibility.Hidden;

                //Show Import Button
                btn_import.Visibility = Visibility.Visible;
                btn_import.Width = 70;
            }
        }

        private void Btn_Import_Click(object sender, RoutedEventArgs e)
        {
            //Before we can start the import we need to verify a few details
            //Check that the Creator column is valid
            if(F95Scanner._GameDetailList.Count > 0)
            {
                if(F95Scanner._GameDetailList.Any(x=>x.Creator != string.Empty) &&
                    F95Scanner._GameDetailList.Any(x => x.Title != string.Empty)
                )
                {
                    EmitImportSignal();
                    this.Close();
                }
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
