using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Atlas.UI
{
    public static class InterfaceHelper
    {
        public static ListBox Listbox { get; set; }
        public static DataGrid Datagrid { get; set; }
        public static ProgressBar GameScannerProgressBar { get; set; }

        public static TextBox PotentialGamesTextBox { get; set; }
        public static ProgressBar LauncherProgressBar { get; internal set; }
        public static ProgressBar UpdateProgressBar { get; internal set; }
        public static double ProgressBarStartValue { get; set; }
        public static TextBox UpdateTextBox { get; internal set; }
        public static TextBox LauncherTextBox { get; internal set; }
        public static Launcher LauncherWindow { get; internal set; }
        public static TextBlock ImporterScanTextBox { get; set; }
        public static ListView BannerView { get; set; }
        public static Window MainWindow { get; set; }
        public static Dispatcher Dispatcher { get; internal set; }
        public static Grid GameImportBox { get; internal set; }
        public static Label GameImportTextBox { get; internal set; }
        public static ProgressBar GameImportPB { get; internal set; }
        public static Label GameImportPBStatus { get; internal set; }
    }
}
