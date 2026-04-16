namespace Mozart.Metadata;

public enum MusicState : byte
{
    Ready       = 0x00, // 0
    NoMusic     = 0x01, // 1
    NoAccess    = 0x02, // 2
    Downloading = 0x04  // 4
}
