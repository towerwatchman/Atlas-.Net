using Atlas.Core;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

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
            tb_format.Text = Atlas.Core.Settings.Config.FolderStructure;
            OtherDefaultExt.Text = $"{Extensions.win_exe},{Extensions.oth_exe}";
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
