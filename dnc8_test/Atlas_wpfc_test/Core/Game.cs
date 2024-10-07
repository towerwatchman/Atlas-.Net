using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Atlas.Core
{
    public class Game
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string Engine { get; set; }
        public string Creator { get; set; }
        public string Status { get; set; }
        public BitmapImage ImageData { get; set; }
    }

    public class GameDetails
    {
        public string Title { set; get; }
        public string Creator { set; get; }
        public string Engine { set; get; }
        public string Version { set; get; }
        public List<string> Executable { set; get; }
        public string Folder { set; get; }
    }
}
