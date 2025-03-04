using Atlas.UI.Windows;
using NLog;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Atlas.UI.Pages.Settings
{
    /// <summary>
    /// Interaction logic for AppearancePage.xaml
    /// </summary>
    public partial class AppearancePage : Page
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public AppearancePage()
        {
            InitializeComponent();
            foreach (var item in Directory.GetFiles(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "ui")))
            {
                if (System.IO.Path.GetExtension(item) == ".xaml")
                {
                    cbThemes.Items.Add(System.IO.Path.GetFileNameWithoutExtension(item));
                }
            }

            foreach (var item in Directory.GetFiles(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "banners")))
            {
                if (System.IO.Path.GetExtension(item) == ".xaml")
                {
                    cbBanners.Items.Add(System.IO.Path.GetFileNameWithoutExtension(item));
                }
            }
            //Try to change theme to config file theme
            int index = cbThemes.Items.IndexOf(Atlas.Core.Settings.Config.Theme);            
            cbThemes.SelectedIndex = index;

            index = cbBanners.Items.IndexOf(Atlas.Core.Settings.Config.BannerTheme);
            cbBanners.SelectedIndex = index;
        }

        private void bLoadTheme_Click(object sender, RoutedEventArgs e)
        {
            string ThemeName = cbThemes.Text;
            try
            {
                string theme = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "ui", $"{ThemeName}.xaml");
                var themeUri = new Uri(theme, UriKind.RelativeOrAbsolute);
                if (System.IO.File.Exists(theme))
                {
                    //This is not the best way to do this. We will need to change this
                    Application.Current.Resources.MergedDictionaries.RemoveAt(1);
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = themeUri });

                    Atlas.Core.Settings.Config.Theme = ThemeName;
                }
            }
            catch (Exception ex)
            {
                //Default to regular theme
                Logger.Error(ex);
            }

        }

        private void bOpenXamlEditor_Click(object sender, RoutedEventArgs e)
        {
            XamlEditor editor =  new XamlEditor();
            editor.Show();
        }

        private void cbBanners_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void bLoadBanner_Click(object sender, RoutedEventArgs e)
        {
            string ThemeName = cbBanners.Text;
            try
            {
                string xamlFilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", "banners", $"{ThemeName}.xaml");

                if (System.IO.File.Exists(xamlFilePath))
                {
                    string xamlContent = File.ReadAllText(xamlFilePath);
                    UpdateUserControlContent(xamlFilePath, "Atlas.UI.Pages.BannerViewPage", "Atlas.UI.GameBanner");                    
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
       
        }

        private void UpdateUserControlContent(string xamlFilePath, string pageClassName, string targetControlClassName)
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
        private Frame FindFrame(DependencyObject parent)
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
        private UserControl FindUserControlByClassName(DependencyObject parent, string className)
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
    }
}
