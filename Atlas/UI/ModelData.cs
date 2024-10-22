//This static class will hold each list of game items

using Atlas.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.UI
{
    public static class ModelData
    {
        public static List<Game> Games { get; set; }

        public static int TotalGames { get; set; }
        public static int TotalVersions { get; set;}
    }
}
