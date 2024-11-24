//This static class will hold each list of game items

using Atlas.UI.ViewModel;
using System.Collections.ObjectModel;

namespace Atlas.UI
{
    public static class ModelData
    {
        //public static ObservableCollection<Game> Games { get; set; }

        public static ObservableCollection<GameViewModel> GameCollection { get; set; }

        public static int TotalGames { get; set; }
        public static int TotalVersions { get; set; }
    }
}
