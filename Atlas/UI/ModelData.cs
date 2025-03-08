//This static class will hold each list of game items

using Atlas.UI.ViewModel;
using System.Collections.ObjectModel;

namespace Atlas.UI
{
    public static class ModelData
    {
        public static ObservableCollection<NotificationViewModel> NotificationCollection { get; set; }
        public static int TotalGames { get; set; }
        public static int TotalVersions { get; set; }

        static ModelData()
        {
            NotificationCollection = new ObservableCollection<NotificationViewModel>();
        }
    }
}
