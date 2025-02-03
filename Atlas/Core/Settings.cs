using Config.Net;
using SQLitePCL;
using System.IO;

namespace Atlas.Core
{
    public static class Settings
    {
        public static SettingInterface Config;

        public static void Init()
        {
            //Add link to config
            Console.WriteLine(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.ini"));
            Config = new ConfigurationBuilder<SettingInterface>().UseIniFile(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.ini")).Build();
            //Set all inital values here. 

            //PATHS
            Config.EnginesPath = Config.EnginesPath == null ? "data\\engines" : Config.EnginesPath;
            Config.RootPath = Config.RootPath == null ? Directory.GetCurrentDirectory() : Config.RootPath;
            Config.DatabasePath = Config.DatabasePath == null ? "data" : Config.DatabasePath;
            Config.ImagesPath = Config.ImagesPath == null ? "data\\images" : Config.ImagesPath;
            Config.GamesPath = Config.GamesPath == null ? "data\\games" : Config.GamesPath;
            Config.ThemesPath = Config.ThemesPath == null ? "themes" : Config.ThemesPath;

            //APP
            Config.Theme = Config.Theme == null ? "Dark.xaml" : Config.Theme;
            Config.ShowListView = Config.FTS != true ? true : Config.ShowListView;

            //IMPORTER
            Config.FolderStructure = Config.FolderStructure == null ? @"{Creator}\{Title}\{Version}" : Config.FolderStructure;
            Config.ImageRenderHeight = Config.ImageRenderHeight == 0 ? 251 : Config.ImageRenderHeight;
            Config.ImageRenderWidth = Config.ImageRenderWidth == 0 ? 537 : Config.ImageRenderWidth;
            Config.DefaultPage = Config.DefaultPage == "" ? "VNHGames" : Config.DefaultPage;
            Config.ExecutableExt = Config.ExecutableExt == "" ? ".zip,.7z,.rar" : Config.ExecutableExt;
            Config.ExtractionExt = Config.ExtractionExt == "" ? ".exe,.swf,.flv,.f4v,.rag,.cmd,.bat,.jar,.html" : Config.ExtractionExt;
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
        [Option(Alias = "app.FTS")]
        bool FTS { get; set; }
        [Option(Alias = "app.Theme")]
        string Theme { get; set; }
        [Option(Alias = "app.ShowListView")]
        bool ShowListView { get; set; }
        #endregion

        #region IMPORTER
        [Option(Alias = "importer.FolderStructure")]
        string FolderStructure { get; set; }
        [Option(Alias = "importer.ExtractionExt")]
        string ExtractionExt { get; set; }
        [Option(Alias = "importer.ExecutableExt")]
        string ExecutableExt { get; set; }
        #endregion

        #region UI
        [Option(Alias = "ui.ImageRenderHeight")]
        double ImageRenderHeight { get; set; }
        [Option(Alias = "ui.ImageRenderWidth")]
        double ImageRenderWidth { get; set; }
        [Option(Alias = "ui.DefaultPage")]
        string DefaultPage { get; set; }

        #endregion
    }

    public static class Global
    {
        public static bool DeleteAfterImport { get; set; }
    }
}
