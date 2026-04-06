namespace Mozart.Data.Entities;

public enum FreePassType : uint
{
    None              = 0x00000000,

    /// <summary>
    /// Unlock all music without any time limit. Expiry is ignored in this type.<br/>
    /// (The status will not be displayed on the player status page)
    /// </summary>
    AllMusic          = 0x00000001, // 1

    /// <summary>
    /// This will not unlock any music.<br/>
    /// (The status will still be displayed on the player status page)
    /// </summary>
    UnusedFreePlay1   = 0x00000002, // 2

    /// <summary>
    /// Unlock all music marked as new with time limit.
    /// </summary>
    NewMusic          = 0x00000004, // 4

    /// <summary>
    /// Time limited access to songs in the acquired music table.
    /// </summary>
    FreePlay          = 0x00000008, // 8

    /// <summary>
    /// Time limited access to songs in the acquired music table.
    /// </summary>
    FreePlayAlt1      = 0x00000010, // 16

    /// <summary>
    /// Time limited access to TOP 100 songs in the acquired music table.<br/>
    /// Can be combined with starter music set.
    /// </summary>
    Top100            = 0x00000020, // 32

    /// <summary>
    /// All music unlocked with time limit.
    /// </summary>
    PremiumTime       = 0x00000040, // 64

    /// <summary>
    /// This will not unlock any music.<br/>
    /// (The status will still be displayed on the player status page)
    /// </summary>
    UnusedFreePlay2   = 0x00000080, // 128

    /// <summary>
    /// Time limited access to TOP 100 songs in the acquired music table.<br/>
    /// </summary>
    Top100Extended    = 0x00000100, // 256

    /// <summary>
    /// Time limited access to songs in the acquired music table.
    /// </summary>
    FreePlayAlt2      = 0x00000200, // 512

    /// <summary>
    /// This will not unlock any music.<br/>
    /// (The status will still be displayed on the player status page)
    /// </summary>
    UnusedFreePlay3   = 0x00000400, // 1024

    /// <summary>
    /// Time limited access to songs in the acquired music table.
    /// </summary>
    FreePlayAlt3      = 0x00000420, // 1056
}
