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
                Updater.CheckForUpdates(pbSplash);
            });          
            await Task.WhenAll(t);

            //Set folders
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "games"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "images"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "logs"));
            Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "data", "updates"));
            pbSplash.Value += 10;
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
