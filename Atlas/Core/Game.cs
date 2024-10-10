using System.Windows.Media.Imaging;

namespace Atlas.Core
{
    public class Game
    {
        public required string Title { get; set; }
        public required string Creator { get; set; }
        public required string Engine { get; set; }
        public required List<GameVersion> Versions  { get; set; }
        public required string Status { get; set; }
        public required BitmapImage ImageData { get; set; }
    }

    public class GameDetails
    {
        public required string Title { set; get; }
        public required string Creator { set; get; }
        public required string Engine { set; get; }
        public required string Version { set; get; }
        public required string Folder { set; get; }

        //For displaying combo box correctly
        public required List<string> Executable { set; get; }
        private string _Text;
        public string Text
        {
            get
            {
                if (Executable.Count > 0)
                {
                    _Text = Executable[0];
                }
                return _Text;
            }
            set { _Text = value; }
        }
        private int _SelectedValue = 0;
        public int SelectedValue
        {
            get { return _SelectedValue; }
            set { _SelectedValue = value; }
        }
        public int SelectedIndex = 0;
    }
}
