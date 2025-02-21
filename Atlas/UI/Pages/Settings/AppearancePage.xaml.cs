using NLog;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
            foreach (var item in Directory.GetFiles(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes")))
            {
                if (System.IO.Path.GetExtension(item) == ".xaml")
                {
                    cbThemes.Items.Add(System.IO.Path.GetFileNameWithoutExtension(item));
                }
            }
            //Try to change theme to config file theme
            int index = cbThemes.Items.IndexOf(Atlas.Core.Settings.Config.Theme);            
            cbThemes.SelectedIndex = index;

        }

        private void bLoadTheme_Click(object sender, RoutedEventArgs e)
        {
            string ThemeName = cbThemes.Text;
            try
            {
                string theme = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes", $"{ThemeName}.xaml");
                var themeUri = new Uri(theme, UriKind.RelativeOrAbsolute);
                if (System.IO.File.Exists(theme))
                {
                    //This is not the best way to do this. We will need to change this
                    Application.Current.Resources.MergedDictionaries.RemoveAt(2);
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
    }
}
