using Atlas.Core;
using Atlas.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Atlas.UI.ViewModel
{
    public sealed class GameViewModel
    {
        public GameViewModel(Game game) 
        {
            _recordID = game.RecordID;
            _title = game.Title;
            _creator = game.Creator;
            _engine = game.Engine;
            _versions = game.Versions;
            _status = game.Status;
            _likes = game.Likes;
            _favorites = game.Favorites;
            _views = game.Views;
            _category = game.Category;
            _overview = game.Overview;
            _os = game.Os;
            _isFavorite = game.IsFavorite;
            _bannerPath = game.BannerPath;
            _siteUrl = game.SiteUrl;
            _screens = game.Screens;
            _tags = game.Tags;

        }
        public int _recordID { get; private set; }
        public int _atlasID { get; private set; }
        public int _f95_id { get; private set; }
        public string _title { get; private set; }
        public string _creator { get; private set; }
        public string _engine { get; private set; }
        public List<GameVersion> _versions { get; private set; }
        public string _status { get; private set; }
        public string _likes { get; private set; }
        public string _favorites { get; private set; }
        public string _views { get; private set; }
        public string _category { get; private set; }
        public string _overview { get; private set; }
        public string _os { get; private set; }
        public bool _isFavorite { get; private set; }
        public string _bannerPath { get; private set; }
        public string _siteUrl { get; private set; }
        public string[] _screens { get; private set; }
        public string _tags { get; private set; }

        public BitmapImage BannerImage()
        {
            ImageInterface image = new ImageInterface();
            return image.LoadImage(_bannerPath, Atlas.Core.Settings.Config.ImageRenderWidth);
        }
        BitmapImage _bannerImage;
    }
}
