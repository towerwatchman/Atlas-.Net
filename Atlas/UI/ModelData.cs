//This static class will hold each list of game items

using Atlas.Core;
using Atlas.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.UI
{
    public static class ModelData
    {
        //public static ObservableCollection<Game> Games { get; set; }

        public static ObservableCollection<GameViewModel> GameCollection { get; set; }

        public static int TotalGames { get; set; }
        public static int TotalVersions { get; set;}
    }
}
