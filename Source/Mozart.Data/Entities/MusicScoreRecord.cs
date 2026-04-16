using Mozart.Metadata;

namespace Mozart.Data.Entities;

public class MusicScoreRecord
{
    public int UserId { get; init; }

    public int MusicId { get; init; }

    public Difficulty Difficulty { get; init; }

    public uint Score { get; set; }

    public ClearType ClearType { get; set; }
}
