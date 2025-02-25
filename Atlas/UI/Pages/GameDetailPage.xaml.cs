﻿using Atlas.Core;
using Atlas.Core.Utilities;
using Atlas.UI.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

namespace Atlas.UI.Pages
{
    /// <summary>
    /// Interaction logic for GameDetailPage.xaml
    /// </summary>
    public partial class GameDetailPage : Page
    {
        private GameViewModel CurrentGame { get; set; }
        public string ImageUriAnimated { get; set; }
        public GameDetailPage(GameViewModel game)
        {
            CurrentGame = game;
            InitializeComponent();
            ShowVersions.Visibility = Visibility.Hidden;
            VersionsList.Visibility = Visibility.Hidden;
            _ = BuildGameDetailAsync();

        }

        private async Task BuildGameDetailAsync()
        {
            if (CurrentGame != null)
            {

                BitmapSource ImageData = await ImageInterface.LoadImageAsync(CurrentGame.RecordID, CurrentGame.BannerPath, 1000, 250);
                if (ImageData != null)
                {
                    ImageData.Freeze();
                    if (System.IO.Path.GetExtension(CurrentGame.BannerPath) == ".gif")
                    {
                        AnimationBehavior.SetSourceUri(AnimatedBanner, new System.Uri(CurrentGame.BannerPath));
                    }
                    else
                    {
                        banner_main.Source = ImageData;

                    }
                    banner_background.Source = ImageData;
                }

                GameTitle.Content = CurrentGame.Title;
                //banner_left.Source = ImageData;
                //banner_right.Source = ImageData;
                //banner_background.Source = ImageData;
                CurrentVersion.Content = $"{CurrentGame.Versions[0].Version}";
                if (CurrentGame.Versions.Count > 1)
                {
                    ShowVersions.Visibility = Visibility.Visible;
                }

            }
        }

        private void ShowVersions_Click(object sender, RoutedEventArgs e)
        {
            var game = CurrentGame;
            int buttonHeight = 20;
            if (game != null)
            {
                //List<Button> menuitems = new List<Button>();
                if (game.Versions != null)
                {
                    VersionsList.Visibility = Visibility.Visible;
                    int index = 1;
                    var rowDefinition = new RowDefinition();
                    rowDefinition.Height = GridLength.Auto;
                    VersionsList.RowDefinitions.Add(rowDefinition);
                    foreach (GameVersion version in game.Versions)
                    {
                        Button button = new Button();
                        button.Content = version.Version;
                        button.Width = 180;                        
                        button.Height = 20;
                        button.HorizontalContentAlignment = HorizontalAlignment.Center;
                        button.Style = new Style();
                        Grid.SetColumn(button, index);
                        Grid.SetRow(button, index);
                        VersionsList.Children.Add(button);
                        index++;
                        rowDefinition = new RowDefinition();
                        rowDefinition.Height = GridLength.Auto;
                        VersionsList.RowDefinitions.Add(rowDefinition);
                    }

                    VersionsList.Height = (buttonHeight * index) +20;
                }
                //bvp.miPlay.ItemsSource = menuitems;
            }
        }

        private void ShowVersions_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VersionsList.Visibility = Visibility.Hidden;
        }

        private void VersionsList_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VersionsList.Visibility = Visibility.Visible;
        }

        private void VersionsList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            VersionsList.Visibility = Visibility.Hidden;
        }
    }
}
