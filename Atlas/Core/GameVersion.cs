using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.Core
{
    class GameVersion
    {
        public required int Id {  get; set; }
        public required string Version { get; set; }
        public required string GamePath { get; set;}
        public required string ExePath { get; set;}
        public required DateTime DateAdded { get; set; }
        public required int Playtime { get; set;} //In Minutes
        public required double FolderSize { get; set; }
    }
}
