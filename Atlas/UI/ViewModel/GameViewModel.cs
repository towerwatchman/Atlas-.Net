using Atlas.Core;
using Atlas.Core.Utilities;
using NLog;
using NLog.LayoutRenderers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using Windows.Services.Maps.LocalSearch;

namespace Atlas.UI.ViewModel
{
    public sealed class GameViewModel : INotifyPropertyChanged
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public GameViewModel(Game game)
        {
            RecordID = game.RecordID;
            AtlasID = game.AtlasID;
            F95ID = game.F95ID;
            Title = game.Title;
            Creator = game.Creator;
            Engine = game.Engine;
            Versions = game.Versions;
            Status = game.Status;
            Likes = game.Likes;
            Favorites = game.Favorites;
            Views = game.Views;
            Category = game.Category;
            Overview = game.Overview;
            OS = game.OS;
            IsFavorite = game.IsFavorite;
            BannerPath = game.BannerPath;
            SiteUrl = game.SiteUrl;
            Screens = game.Screens;
            Tags = game.Tags;
            Collection = game.Collection;
            CurrentSelectedVersion = game.CurrentSelectedVersion;
            Rating = game.Rating;
            LatestVersion = game.LatestVersion;
            Censored = game.Censored;
            Language = game.Language;
            Genre = game.Genre;
            ReleaseDate = game.ReleaseDate;
            Translations = game.Translations;
            Voice = game.Voice;
            ShortName = game.ShortName;

        }
        private bool _isUpdateAvailable;
        public int RecordID { get; private set; }
        public int AtlasID { get; private set; }
        public int F95ID { get; private set; }
        public string Title { get; set; }
        public string Creator { get; set; }
        public string Engine { get; set; }
        public List<GameVersion> Versions { get; set; }
        public string Status { get; set; }
        public string Likes { get; private set; }
        public string Favorites { get; private set; }
        public string Views { get; private set; }
        public string Category { get; private set; }
        public string Overview { get; private set; }
        public string OS { get; private set; }
        public bool IsFavorite { get; private set; }
        public string BannerPath { get; set; }
        public string ImageUriAnimated { get; set; }
        public string SiteUrl { get; private set; }
        public string[] Screens { get; private set; }
        public string Tags { get; private set; }
        public string Collection { get; private set; }
        public string CurrentSelectedVersion { get; set; }
        public string Rating { get; set; }
        public string LatestVersion { get; set; }
        public string Censored { get; set; }
        public string Language { get; set; }

        public bool IsUpdateAvailable
        {
            get
            {
                Task.Run(() =>
                {
                    bool isUpdateAvailable = false;
                    int latest = 0;
                    try
                    {
                        latest = Convert.ToInt32(Regex.Replace(LatestVersion, "[^0-9]", ""));
                    }
                    catch
                    {

                    }
                    foreach (var version in Versions)
                    {
                        int current = 0;
                        try
                        {
                            current = Convert.ToInt32(Regex.Replace(version.Version, "[^0-9]", ""));
                        }
                        catch { }
                        if (latest > current)
                        {
                            isUpdateAvailable = true;
                        }
                        else
                        {
                            isUpdateAvailable = false;
                            break;
                        }
                    }
                    if (isUpdateAvailable != _isUpdateAvailable)
                    {
                        _isUpdateAvailable = isUpdateAvailable;
                        OnPropertyChanged(nameof(IsUpdateAvailable));
                    }
                });

                return _isUpdateAvailable;
            }
            set
            {
                _isUpdateAvailable = value;
                OnPropertyChanged(nameof(IsUpdateAvailable));
            }
        }
        public BitmapSource BannerImage
        {
            get
            {
                if (_bannerImage == null)
                {
                    //BannersInView.Add(RecordID);
                    //return _bannerImage;
                    Task.Run(async () =>
                    {
                        BitmapSource bi = await ImageInterface.LoadImageAsync(RecordID, BannerPath, Atlas.Core.Settings.Config.ImageRenderWidth);
                        if (bi != null)
                        {

                            bi.Freeze();
                            _bannerImage = bi;
                            bi = null;

                            OnPropertyChanged("BannerImage");

                            //Logger.Warn($"Loaded Image for id: {RecordID}");
                        }
                    });

                    return _bannerImage;
                }
                return _bannerImage;
            }
            set { _bannerImage = value; }
        }

        public string Genre { get; set; }
        public string ReleaseDate { get; set; }
        public string Translations { get; set; }
        public string Voice { get; internal set; }
        public string ShortName { get; internal set; }

        BitmapSource _bannerImage;

        public static List<int> BannersInView = new List<int>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Logger.Warn("Property Changed");
        }
    }
}
