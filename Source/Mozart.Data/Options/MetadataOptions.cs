namespace Mozart.Options;

public class MetadataOptions
{
    public const string Section = "Metadata";

    public string MusicList { get; init; } = string.Empty;
    public string AlbumList { get; init; } = string.Empty;
    public string ItemData  { get; init; } = string.Empty;
}