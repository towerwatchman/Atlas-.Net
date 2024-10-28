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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Atlas.UI.Pages.Importers
{
    /// <summary>
    /// Interaction logic for Custom.xaml
    /// </summary>
    public partial class Custom : Page
    {
        public Custom()
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
        private void cb_format_Click(object sender, RoutedEventArgs e)
        {
            tb_format.IsEnabled = !(bool)cb_format.IsChecked;
        }

        private void tb_FolderDialog_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Check if the current text in the Root Path is a valid folder
            //btn_next.IsEnabled = Directory.Exists(tb_FolderDialog.Text);
        }
    }
}
