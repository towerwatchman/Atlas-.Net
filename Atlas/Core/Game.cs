using System.Windows;
using System.Windows.Media.Imaging;

namespace Atlas.Core
{
    public class Game
    {
        public required int RecordID { get; set; }
        public string AtlasID { get; set; }
        public string F95ID { get; set; }
        public required string Title { get; set; }
        public required string Creator { get; set; }
        public required string Engine { get; set; }
        public required List<GameVersion> Versions  { get; set; }
        public string Status { get; set; }
        public required BitmapImage ImageData { get; set; }
    }

    public class GameDetails
    {
        public string Id { get; set; }  
        public required string Title { set; get; }
        public required string Creator { set; get; }
        public required string Engine { set; get; }
        public required string Version { set; get; }
        public required string Folder { set; get; }
        public required Visibility SingleEngineVisible { set; get; }
        public required Visibility MultipleEngineVisible { set; get; }
        public required Visibility ResultVisibilityState { set; get; }

        //For displaying Executable combo box correctly
        public required string SingleExecutable { set; get; }
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

        //For displaying Results combo box correctly
        public required List<string> Results { set; get; }
        private string _ResultText;
        public string ResultText
        {
            get
            {
                if (Results.Count > 0)
                {
                    _ResultText = Results[0];
                }
                return _ResultText;
            }
            set { _ResultText = value; }
        }
        private int _ResultSelectedValue = 0;
        public int ResultSelectedValue
        {
            get { return _ResultSelectedValue; }
            set { _ResultSelectedValue = value; }
        }
        public int ResultSelectedIndex = 0;
    }
}
