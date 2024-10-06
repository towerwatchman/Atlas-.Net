using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Atlas.Core
{
    class Game
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string Engine { get; set; }
        public string Creator { get; set; }
        public string Status { get; set; }
        public BitmapImage ImageData { get; set; }
    }
}
