namespace Mozart.Metadata;

public enum GameMode : byte
{
    // New Mode
    Normal = 0,
    Live = 1,

    // Legacy mode
    Single,
    Versus,
    Jam,
    Couple
}
