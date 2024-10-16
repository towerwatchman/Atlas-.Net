using Config.Net;
using System.IO;

namespace Atlas.Core
{
    public static class Settings
    {
        public static SettingInterface Config;

        public static void Init()
        { 
            //Add link to config
            Config = new ConfigurationBuilder<SettingInterface>().UseIniFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.ini")).Build();
            //Set all inital values here. 
            Config.ImageRenderHeight = Config.ImageRenderHeight == 0 ? 251 : Config.ImageRenderHeight;
            Config.ImageRenderWidth = Config.ImageRenderWidth == 0 ? 537 : Config.ImageRenderWidth;
        }

    }
    public interface SettingInterface
    {
        #region PATHS
        [Option(Alias = "paths.EnginesPath")]
        string EnginesPath { get; set; }
        [Option(Alias = "paths.RootPath")]
        string RootPath { get; set; }
        [Option(Alias = "paths.DatabasePath")]
        string DatabasePath { get; set; }
        [Option(Alias = "paths.ImagesPath")]
        string ImagesPath { get; set; }
        [Option(Alias = "paths.GamesPath")]
        string GamesPath { get; set; }
        [Option(Alias = "paths.ThemesPath")]
        string ThemesPath { get; set; }
        #endregion

        #region APP
        [Option(Alias = "app.Theme")]
        string Theme { get; set; }
        [Option(Alias = "app.ShowListView")]
        bool ShowListView { get; set; }
        #endregion

        #region IMPORTER
        [Option(Alias = "importer.FolderStructure")]
        string FolderStructure { get; set; }
        #endregion

        #region UI
        [Option(Alias = "ui.ImageRenderHeight")]
        double ImageRenderHeight { get; set; }
        [Option(Alias = "ui.ImageRenderWidth")]
        double ImageRenderWidth { get; set; }

        #endregion
    }
}
