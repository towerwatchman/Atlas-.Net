using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Atlas.Core;

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
            foreach (var item in Directory.GetFiles(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "themes")))
            {
                if(System.IO.Path.GetExtension(item) == ".xaml")
                {
                    cbThemes.Items.Add(System.IO.Path.GetFileNameWithoutExtension(item));
                }
            }
            cbThemes.SelectedIndex = 0;

        }

        private void bLoadTheme_Click(object sender, RoutedEventArgs e)
        {
            string ThemeName = cbThemes.Text;
            try
            {
                string theme = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "themes", $"{ThemeName}.xaml");
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
