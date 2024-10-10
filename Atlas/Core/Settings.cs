using Config.Net;

namespace Atlas.Core
{
    public static class Settings
    {
        public static SettingInterface Config;
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
        [Option(Alias = "Importer.FolderStructure")]
        string FolderStructure { get; set; }
        #endregion

    }
}
