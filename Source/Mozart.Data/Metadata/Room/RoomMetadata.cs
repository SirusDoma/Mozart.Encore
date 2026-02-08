namespace Mozart.Metadata.Room;

public class RoomMetadata : ICloneable
{
    public required int Id { get; init; }

    public required string Title { get; set; }

    public required GameMode Mode { get; set; }

    public required int MusicId { get; set; }

    public required Difficulty Difficulty { get; set; }

    public required GameSpeed Speed { get; set; }

    public required int MinLevelLimit { get; init; }

    public required int MaxLevelLimit { get; init; }

    public required Arena Arena { get; set; }

    public byte ArenaRandomSeed { get; set; }

    public IList<int> Skills { get; set; } = [];

    public int SkillsSeed { get; set; }

    public string Password { get; init; } = string.Empty;

    public RoomState State { get; set; } = RoomState.Waiting;

    public object Clone()
    {
        return MemberwiseClone();
    }
}
