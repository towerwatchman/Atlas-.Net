using Atlas.Core;
using Atlas.Core.Database;
using Atlas.UI.ViewModel;
using Atlas.UI.Windows;
using NLog;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace Atlas.UI.Pages

{
    /// <summary>
    /// Interaction logic for BannerViewPage.xaml
    /// </summary>
    public partial class BannerViewPage : Page
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool isGameDetailShown = false;
        public ObservableCollection<GameViewModel> GameCollection { get; set; }

        public BannerViewPage()
        {
            InitializeComponent();
            GameCollection = new ObservableCollection<GameViewModel>();
            DataContext = this;
            BannerView.ItemsSource = GameCollection;
            LoadAllGames();
        }

        private async void LoadAllGames()
        {
            var games = await SQLiteInterface.BuildGameListDetails();
            foreach (var game in games)
            {
                GameCollection.Add(game);
            }
            Logger.Info($"Loaded all {GameCollection.Count} games into GameCollection");
        }

        // Optional: Handle dynamic growth
        public async void RefreshGames()
        {
            GameCollection.Clear();
            var games = await SQLiteInterface.BuildGameListDetails();
            foreach (var game in games)
            {
                GameCollection.Add(game);
            }
            Logger.Info($"Refreshed with {GameCollection.Count} games");
        }

        // Optional: Add a new game with a cap
        public void AddGame(Game game)
        {
            if (GameCollection.Count >= 5000) // Example cap
            {
                GameCollection.RemoveAt(0); // Remove oldest
            }
            GameCollection.Add(new GameViewModel(game));
            Logger.Info($"Added game, total: {GameCollection.Count}");
        }
    }
}