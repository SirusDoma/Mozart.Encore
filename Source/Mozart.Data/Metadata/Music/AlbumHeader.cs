namespace Mozart.Metadata.Music;

public class AlbumHeader
{
    public class MusicEntry
    {
        public int Id { get; set; }
        public Difficulty Difficulty { get; set; }
    }

    public int ServerId { get; set; }
    public int AlbumId { get; set; }
    public short MasterId { get; set; }
    public byte[] Name { get; set; } = [];
    public int Level { get; set; }
    public List<MusicEntry> Entries { get; set; } = [];

}