using ABI.System;
using Atlas.Core;
using Atlas.Core.Database;
using Atlas.UI.ViewModel;
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
using Exception = System.Exception;

namespace Atlas.UI.Windows
{
    /// <summary>
    /// Interaction logic for GameDetailWindow.xaml
    /// </summary>
    public partial class GameDetailWindow : Window
    {
        private GameViewModel viewModel;
        public GameDetailWindow(GameViewModel game)
        {
            viewModel = game;
            InitializeComponent();
            title.Text = game.Title;
            short_name.Text = game.ShortName;
            platform.Text = game.OS;
            engine.Text = game.Engine;
            developer.Text = game.Creator;
            release_date.Text = System.DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(game.ReleaseDate)).DateTime.ToString("yyyy-MM-dd");
            status.Text = game.Status;
            tags.Text = game.Tags.Replace(",", " , ");
            description.Text = game.Overview;
            category.Text = game.Category;
            latest_version.Text = game.LatestVersion;
            censored.Text = game.Censored;
            language.Text = game.Language;
            translations.Text = game.Translations;
            genre.Text = game.Genre;
            voice.Text = game.Voice;
            rating.Text = game.Rating;

            foreach (var version in game.Versions)
            {
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = version.Version;
                listBoxItem.Tag = version;
                VersionListBox.Items.Add(listBoxItem);
            }
            VersionListBox.SelectedIndex = 0;

        }


        #region Windows Buttons
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
            this.Close();
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

        private void VersionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // var listBox = (ListBox)sender;

            ListBoxItem listBoxItem = (ListBoxItem)VersionListBox.SelectedItem;
            if (listBoxItem != null)
            {
                GameVersion version = listBoxItem.Tag as GameVersion;

                if (version != null)
                {
                    game_version.Text = version.Version;
                    game_path.Text = version.GamePath;
                    executable.Text = version.ExePath;
                    last_played.Text = version.LastPlayed.ToString();
                    playtime.Text = version.Playtime.ToString();
                    version_size.Text = version.FolderSize.ToString();
                    date_added.Text = version.DateAdded.ToString();
                }
            }

        }

        private void VersionRemove_Click(object sender, RoutedEventArgs e)
        {

            ListBoxItem listBoxItem = (ListBoxItem)VersionListBox.SelectedItem;
            GameVersion gameVersion = listBoxItem.Tag as GameVersion;

            int RecordID = gameVersion.RecordId;
            string version = gameVersion.Version;
            //remove from database
            MessageBoxResult result = MessageBox.Show("This will also remove the game folder. Do you want to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {

                bool isDeleted = SQLiteInterface.RemoveVersion(RecordID, version);

                if (isDeleted)
                {
                    DeleteGameFolder();
                    //remove from data
                    //### FIX HERE
                    /*
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        GameViewModel gameObj = ModelData.GameCollection.Where(x => x.RecordID == RecordID).FirstOrDefault();
                        var index = ModelData.GameCollection.IndexOf(gameObj);

                        if (gameObj != null)
                        {
                            ModelData.GameCollection[index].Versions.Remove(gameVersion);
                            gameObj.OnPropertyChanged("BannerImage");
                        }

                        //reset listbox
                        VersionListBox.Items.Clear();
                        viewModel.Versions.Remove(gameVersion);
                        foreach (var version in viewModel.Versions)
                        {
                            ListBoxItem listBoxItem = new ListBoxItem();
                            listBoxItem.Content = version.Version;
                            listBoxItem.Tag = version;
                            VersionListBox.Items.Add(listBoxItem);
                        }
                    });*/
                }
            }
        }

        private void DeleteGameFolder()
        {
            string folderPath = @"C:\Path\To\GameFolder"; // Replace with the actual path to the game folder

            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                    MessageBox.Show("Game folder deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Game folder not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

}
