using Atlas.UI.ViewModel;
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

        public static void UpdateUserControlContent(string xamlFilePath, string pageClassName, string targetControlClassName)
        {
            try
            {
                // Find the Frame in the MainWindow
                Frame frame = FindFrame(InterfaceHelper.MainWindow);
                if (frame == null)
                {
                    throw new Exception("No Frame found in MainWindow.");
                }

                // Get the current Page from the Frame
                if (frame.Content is Page currentPage && currentPage.GetType().FullName == pageClassName)
                {
                    // Find the target UserControl in the Page
                    UserControl targetControl = FindUserControlByClassName(currentPage, targetControlClassName);
                    if (targetControl != null)
                    {
                        // Read and clean the external XAML
                        string xamlContent = File.ReadAllText(xamlFilePath);
                        xamlContent = Regex.Replace(xamlContent,
                            @"x:Class\s*=\s*""[^""]*""",
                            string.Empty,
                            RegexOptions.IgnoreCase);

                        // Parse the XAML into a UI element
                        FrameworkElement newContent = (FrameworkElement)XamlReader.Parse(xamlContent);

                        // Ensure the update happens on the UI thread
                        InterfaceHelper.Dispatcher.Invoke(() =>
                        {
                            // Clear existing content and set new content
                            targetControl.Content = null; // Force a reset
                            targetControl.Content = newContent;

                            // Force layout update
                            targetControl.UpdateLayout();

                            // Verify the update
                            MessageBox.Show($"Updated control class: {targetControl.GetType().FullName}");
                        }, DispatcherPriority.Render);
                    }
                    else
                    {
                        throw new Exception($"UserControl '{targetControlClassName}' not found in '{pageClassName}'.");
                    }
                }
                else
                {
                    throw new Exception($"Page '{pageClassName}' is not currently loaded in the Frame.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating UserControl: {ex.Message}");
            }
        }

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
                return;

            var resources = Application.Current.Resources;

            if (string.IsNullOrEmpty(templateName) || templateName.ToLower().Contains("default"))
            {
                // Use named GameBanner template
                BannerView.ItemTemplate = resources["GameBannerTemplate"] as DataTemplate;
            }
            else if (!string.IsNullOrEmpty(externalXamlPath))
            {
                try
                {
                    // Read the raw XAML content from f95.xaml
                    string externalXamlContent = File.ReadAllText(externalXamlPath);

                    // Get the actual assembly name of GameViewModel
                    string assemblyName = typeof(Atlas.UI.ViewModel.GameViewModel).Assembly.GetName().Name;
                    string namespaceName = typeof(Atlas.UI.ViewModel.GameViewModel).Namespace;

                    // Construct a DataTemplate XAML string with dynamic assembly and namespace
                    string templateXaml = $@"
                        <DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                      xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                                      xmlns:vm=""clr-namespace:{namespaceName};assembly={assemblyName}""
                                      DataType=""{{x:Type vm:GameViewModel}}"">
                            {externalXamlContent}
                        </DataTemplate>";

                    Logger.Debug($"Generated XAML: {templateXaml}");

                    // Convert the string to a Stream
                    byte[] byteArray = Encoding.UTF8.GetBytes(templateXaml);
                    using (var stream = new MemoryStream(byteArray))
                    {
                        // Parse the XAML into a DataTemplate
                        var template = (DataTemplate)XamlReader.Load(stream);
                        BannerView.ItemTemplate = template;
                    }

                    // Force refresh of the ListView
                    var itemsSource = BannerView.ItemsSource;
                    BannerView.ItemsSource = null;
                    BannerView.ItemsSource = itemsSource;
                   


                    Logger.Info($"f95 template applied. Items count: {BannerView.Items.Count}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error loading f95 template");
                }
            }
        }
    }
}
