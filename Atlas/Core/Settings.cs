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
            Config = new ConfigurationBuilder<SettingInterface>().UseIniFile(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config.ini")).Build();
            //Set all inital values here. 

            //PATHS
            Config.EnginesPath = Config.EnginesPath == null || Config.EnginesPath == "" ? Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\engines") : Config.EnginesPath;
            Config.RootPath = Config.RootPath == null || Config.RootPath == "" ? System.AppDomain.CurrentDomain.BaseDirectory : Config.RootPath;
            Config.DatabasePath = Config.DatabasePath == null || Config.DatabasePath == "" ? Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data") : Config.DatabasePath;
            Config.ImagesPath = Config.ImagesPath == null || Config.ImagesPath == "" ? Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\images") : Config.ImagesPath;
            Config.GamesPath = Config.GamesPath == null || Config.GamesPath == "" ? Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data\\games") : Config.GamesPath;
            Config.ThemesPath = Config.ThemesPath == null || Config.ThemesPath == "" ? Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "themes") : Config.ThemesPath;

            //APP
            Config.Theme = Config.Theme == null || Config.Theme == "" ? "Dark.xaml" : Config.Theme;
            Config.ShowListView = Config.FTS != true ? true : Config.ShowListView;

            //IMPORTER
            Config.FolderStructure = Config.FolderStructure ?? @"{Creator}\{Title}\{Version}";
            Config.ImageRenderHeight = Config.ImageRenderHeight == 0 ? 251 : Config.ImageRenderHeight;
            Config.ImageRenderWidth = Config.ImageRenderWidth == 0 ? 537 : Config.ImageRenderWidth;
            Config.DefaultPage = Config.DefaultPage == null || Config.DefaultPage == ""? "VNHGames" : Config.DefaultPage;
            Config.ExecutableExt = Config.ExecutableExt == null || Config.ExecutableExt == "" ? ".exe,.swf,.flv,.f4v,.rag,.cmd,.bat,.jar,.html" : Config.ExecutableExt;
            Config.ExtractionExt = Config.ExtractionExt == null || Config.ExtractionExt == "" ? ".zip,.7z,.rar" : Config.ExtractionExt;

            //DEV
            Config.ShowDebugWindow = Config.ShowDebugWindow == true ? true : Config.ShowDebugWindow;
        }

        //Global vars stored at runtime currently
        public static bool DownloadImages { get; set; }
        public static bool MoveGameOnImport { get; set; }
        public static bool ScanFolderSize { get; set; }
        public static bool DeleteFolderAfterExtraction { get; set; }

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

        #region DEV
        [Option(Alias = "dev.ShowDebugWindow")]
        bool ShowDebugWindow { get; set; }
        #endregion


    }

    public static class Global
    {
        public static bool DeleteAfterImport { get; set; }
    }
}
