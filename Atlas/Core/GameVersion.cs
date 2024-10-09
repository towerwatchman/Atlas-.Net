namespace Atlas.Core
{
    public class GameVersion
    {
        public required int RecordId { get; set; }
        public required string Version { get; set; }
        public required string GamePath { get; set; }
        public required string ExePath { get; set; }
        public required DateTime DateAdded { get; set; }
        public int Playtime { get; set; } //In Minutes
        public double FolderSize { get; set; }
        public DateTime LastPlayed { get; set; }
    }
}
