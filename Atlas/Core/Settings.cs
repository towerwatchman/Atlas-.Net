using Config.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
