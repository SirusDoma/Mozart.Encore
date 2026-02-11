namespace Mozart.Data.Entities;

public class AcquiredMusic
{
    public int UserId { get; init; }

    public required int MusicId { get; init; }
}