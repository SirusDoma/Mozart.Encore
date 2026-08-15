namespace Mozart.Options;

public class GameOptions
{
    public const string Section = "Game";

    public bool AllowSoloInVersus { get; init; } = true;
    public int MusicLoadTimeout { get; init; } = 60;
    public bool FreeMission { get; init; } = true;
}
