namespace Mozart.Metadata.Music;

public class MusicHeader
{
    public int Id { get; set; }
    public byte[] Signature { get; set; } = [];
    public float EncodingVersion { get; set; }
    public Genre Genre { get; set; }
    public float Bpm { get; set; }

    public short LevelEx { get; set; }
    public short LevelNx { get; set; }
    public short LevelHx { get; set; }

    public short Padding { get; set; }

    public int EventCountEx { get; set; }
    public int EventCountNx { get; set; }
    public int EventCountHx { get; set; }

    public int NoteCountEx { get; set; }
    public int NoteCountNx { get; set; }
    public int NoteCountHx { get; set; }

    public int MeasureCountEx { get; set; }
    public int MeasureCountNx { get; set; }
    public int MeasureCountHx { get; set; }

    public int BlockCountEx { get; set; }
    public int BlockCountNx { get; set; }
    public int BlockCountHx { get; set; }

    public short OldEncodingVersion { get; set; }
    public short OldSongId { get; set; }
    public byte[] OldGenre { get; set; } = [];

    public int ThumbnailSize { get; set; }

    public int FileVersion { get; set; }

    public byte[] Title { get; set; } = [];
    public byte[] Artist { get; set; } = [];
    public byte[] NoteDesigner { get; set; } = [];

    public string OJM { get; set; } = string.Empty;

    public int CoverSize { get; set; }

    public int DurationEx { get; set; }
    public int DurationNx { get; set; }
    public int DurationHx { get; set; }

    public int BlockOffsetEx { get; set; }
    public int BlockOffsetNx { get; set; }
    public int BlockOffsetHx { get; set; }

    public int CoverOffset { get; set; }

    public bool Encrypted { get; set; }

    public bool IsNew { get; set; }

    public bool IsPurchasable { get; set; }

    public DateOnly ReleaseDate { get; set; }
}