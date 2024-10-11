using System.Windows.Controls;

namespace Atlas.UI
{
    public static class InterfaceHelper
    {
        public static ListBox Listbox { get; set; }
        public static DataGrid Datagrid { get; set; }
        public static ProgressBar GameScannerProgressBar { get; set; }

        public static TextBox PotentialGamesTextBox { get; set; }
        public static ProgressBar SplashProgressBar { get; internal set; }
        public static TextBox SplashTextBox { get; internal set; }
        public static Splash SplashWindow { get; internal set; }
    }
}
