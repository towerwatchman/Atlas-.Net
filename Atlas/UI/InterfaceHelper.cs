using Atlas.UI.ViewModel;
using Castle.Core.Logging;
using NLog;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Atlas.UI
{
    public static class InterfaceHelper
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
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
        public static ListView NotificationsPage { get; internal set; }

        //Custom functions for modifying UI

       

        // Helper method to find a Frame in the visual tree
        public static Frame FindFrame(DependencyObject parent)
        {
            if (parent == null) return null;

            if (parent is Frame frame) return frame;

            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                Frame result = FindFrame(child);
                if (result != null) return result;
            }
            return null;
        }

        // Helper method to find a UserControl by class name
        public static UserControl FindUserControlByClassName(DependencyObject parent, string className)
        {
            if (parent == null) return null;

            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is UserControl userControl && userControl.GetType().FullName == className)
                {
                    return userControl;
                }

                UserControl result = FindUserControlByClassName(child, className);
                if (result != null) return result;
            }
            return null;
        }
        public static void SetBannerTemplate(string templateName = null, string externalXamlPath = null)
        {
            if (BannerView == null)
            {
                Logger.Warn("BannerView is null, cannot set template.");
                return;
            }

            var resources = Application.Current.Resources;

            /*if (string.IsNullOrEmpty(templateName) || templateName.ToLower() == "default")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    BannerView.ItemTemplate = null;
                    BannerView.ItemTemplate = resources["GameBannerTemplate"] as DataTemplate;
                    Logger.Info("Switched to default GameBannerTemplate");
                    BannerView.Items.Refresh();
                    BannerView.UpdateLayout();
                });
            }
            else */
            if (!string.IsNullOrEmpty(externalXamlPath))
            {
                try
                {
                    // Validate the path
                    Logger.Debug($"Attempting to read file at: {externalXamlPath}");
                    if (!File.Exists(externalXamlPath))
                    {
                        Logger.Error($"File not found: {externalXamlPath}");
                        return;
                    }

                    string externalXamlContent = File.ReadAllText(externalXamlPath);
                    Logger.Debug($"Read XAML content: {externalXamlContent.Substring(0, Math.Min(100, externalXamlContent.Length))}..."); // Log first 100 chars

                    string assemblyName = typeof(Atlas.UI.ViewModel.GameViewModel).Assembly.GetName().Name;
                    string namespaceName = typeof(Atlas.UI.ViewModel.GameViewModel).Namespace;
                    string uiNamespace = "clr-namespace:Atlas.UI;assembly=" + assemblyName; // Adj

                    string templateXaml = $@"
                <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                              xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                              xmlns:vm=""clr-namespace:{namespaceName};assembly={assemblyName}""
                              xmlns:ui=""clr-namespace:Atlas.UI;assembly={assemblyName}""
                              DataType=""{{x:Type vm:GameViewModel}}"">                              
                    <ui:GameBannerControl>
                        <ui:GameBannerControl.Content>
                            {externalXamlContent}
                        </ui:GameBannerControl.Content>
                    </ui:GameBannerControl>
                </DataTemplate>
                    ";

                    Logger.Debug($"Generated XAML: {templateXaml}");

                    byte[] byteArray = Encoding.UTF8.GetBytes(templateXaml);
                    DataTemplate template;
                    using (var stream = new MemoryStream(byteArray))
                    {
                        template = (DataTemplate)XamlReader.Load(stream);
                        Logger.Info($"f95 template created: {template != null}");
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BannerView.ItemTemplate = null;
                        BannerView.ItemTemplate = template;
                        Logger.Debug($"ItemTemplate set: {BannerView.ItemTemplate != null}");
                        BannerView.Items.Refresh();
                        BannerView.UpdateLayout();
                    });

                    Logger.Info($"f95 template applied. Items count: {BannerView.Items.Count}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Error loading f95 template from {externalXamlPath}");
                }
            }
        }
    }
}
