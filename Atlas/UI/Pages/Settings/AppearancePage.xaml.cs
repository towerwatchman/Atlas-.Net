using Atlas.UI.Windows;
using NLog;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

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

            cbBanners.Items.Add("default");
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
            XamlEditor editor = new XamlEditor();
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
                InterfaceHelper.SetBannerTemplate(ThemeName, xamlFilePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }
    }
}
