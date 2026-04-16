namespace Mozart.Metadata;

public enum GameMode : byte
{
    // New mode
    ThreeKeys = 0,
    FiveKeys  = 1,
    SevenKeys = 2,

    // Legacy mode
    Single,
    Versus,
    Jam,
    Couple
}
