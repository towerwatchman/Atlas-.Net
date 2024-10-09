using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Atlas.Core;
using Config.Net;
using NLog;

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
            _= Init();
        }

        public async Task Init()
        {
            //Check for updates
            var t = Task.Run(() =>
            {
                Updater.CheckForUpdates();
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
            Settings.Config = new ConfigurationBuilder<SettingInterface>().UseIniFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(),"config.ini")).Build();

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
