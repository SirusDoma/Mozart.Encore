namespace Mozart.Metadata.Room;

public class RoomMetadata : ICloneable
{
    public required int Id { get; init; }

    public required string Title { get; set; }

    public required KeyMode KeyMode { get; set; }

    public required GameMode GameMode { get; set; }

    public required int MusicId { get; set; }

    public required Difficulty Difficulty { get; set; }

    public required GameSpeed Speed { get; set; }

    public required int MinLevelLimit { get; set; }

    public required int MaxLevelLimit { get; set; }

    public required Arena Arena { get; set; }

    public byte ArenaRandomSeed { get; set; }

    public IList<int> Skills { get; set; } = [];

    public int SkillsSeed { get; set; }

    public string Password { get; set; } = string.Empty;

    public RoomState State { get; set; } = RoomState.Waiting;

    public bool Premium { get; set; }

    public int Type { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
