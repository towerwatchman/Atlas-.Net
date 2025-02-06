using Microsoft.Win32;
using System.Windows.Controls;

namespace Atlas.UI.Pages.Settings
{
    /// <summary>
    /// Interaction logic for LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : Page
    {
        public LibraryPage()
        {
            InitializeComponent();

            tb_AtlasRoot.Text = Atlas.Core.Settings.Config.RootPath;
            tb_GameFolder.Text = Atlas.Core.Settings.Config.GamesPath;
        }

        private void btn_SetGameFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            var result = dialog.ShowDialog();

            var folder = dialog.FolderName;
            if (folder != null)
            {
                tb_GameFolder.Text = folder;
            }
        }

        private void tb_GameFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Task.Run(() =>
            //{
                Atlas.Core.Settings.Config.GamesPath = tb_GameFolder.Text;
            //});

        }
    }
}
