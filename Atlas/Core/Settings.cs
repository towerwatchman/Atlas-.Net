using Config.Net;

namespace Atlas.Core
{
    public static class Settings
    {
        public static SettingInterface? Config;
    }
    public interface SettingInterface
    {
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

        [Option(Alias = "app.Theme")]
        string Theme { get; set; }

    }
}
