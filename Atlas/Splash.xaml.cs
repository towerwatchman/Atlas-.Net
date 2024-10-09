using Atlas.Core;
using Atlas.Core.Database;
using Config.Net;
using System.IO;
using System.Windows;

namespace Atlas
{
    /// <summary>
    /// Check for updates
    /// Set Folders
    /// Add Settings
    /// Run db migrations
    /// </summary>
    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();
            //Discard await warning
            _ = Init();
        }

        public async Task Init()
        {
            //Check for updates
            var t = Task.Run(() =>
            {
                try
                {
                    Updater.CheckForUpdates();
                }
                catch (Exception ex)
                {
                    Logging.Logger.Error(ex);
                }
            });
            await Task.WhenAll(t);
            //.Value += 10;

            //Set folders
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "games"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "images"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "logs"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "updates"));
            //pbSplash.Value += 10;

            //Add Settings
            Settings.Config = new ConfigurationBuilder<SettingInterface>().UseIniFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.ini")).Build();

            //Set the default theme file
            string theme = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "themes", Settings.Config.Theme);
            var themeUri = new Uri(theme, UriKind.RelativeOrAbsolute);
            try
            {
                if (File.Exists(theme))
                {
                    Application.Current.Resources.MergedDictionaries.RemoveAt(2);
                    Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = themeUri });
                }
            }
            catch (Exception ex)
            {
                //Default to regular theme
                Logging.Logger.Error(ex);
            }

            Database.Init();
            //Launch Main Window            
            LaunchMainWindow();
        }

        public void LaunchMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
