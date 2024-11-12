using Atlas.Core;
using Atlas.Core.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        public BitmapSource BannerImage
        {
            get
            {
                //return _bannerImage;
                Task.Run(async () =>
                {
                    BitmapSource bi = await ImageInterface.LoadImage(RecordID, BannerPath, Atlas.Core.Settings.Config.ImageRenderWidth);
                    bi.Freeze();
                    if (!BannersInView.Contains(RecordID))
                    {
                        _bannerImage = bi;
                        
                        //OnPropertyChanged("BannerImage");
                        //Logger.Warn($"Loading Image for id: {RecordID}");
                    }
                    BannersInView.Add(RecordID);
                    //Logger.Warn(Title);
                });
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
            Logger.Warn("Property Changed");
        }
    }
}
