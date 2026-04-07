namespace Mozart.Options;

public class GameOptions
{
    public const string Section = "Game";

    public bool AllowSoloInVersus   { get; init; } = true;

    public int SingleModeRewardLevelLimit { get; init; } = 10;
    public int MusicLoadTimeout { get; init; } = 60;
}
