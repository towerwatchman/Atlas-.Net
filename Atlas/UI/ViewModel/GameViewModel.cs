using Atlas.Core;
using Atlas.Core.Utilities;
using NLog;
using System.ComponentModel;
using System.Windows.Media.Imaging;

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

        }
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

        BitmapSource _bannerImage;

        public static List<int> BannersInView = new List<int>();

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //Logger.Warn($"Property Changed: {propertyName}");
        }
    }
}
