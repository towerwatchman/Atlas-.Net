using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Atlas.Core;
using System.Collections.ObjectModel;

namespace Atlas.UI.Importer
{
    /// <summary>
    /// Interaction logic for BatchImporter.xaml
    /// </summary>
    public partial class BatchImporter : Window
    {
        public BatchImporter()
        {
            InitializeComponent();
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
                GameList.ItemsSource = GameScanner.GameDetailList;
                InterfaceHelper.Datagrid = GameList;

                string folder = tb_FolderDialog.Text;

                Task.Run(() =>
                {
                    GameScanner.Start(folder);
                });

                //GameList.Items.Refresh();
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
            this.Close();
        }
    }
}
