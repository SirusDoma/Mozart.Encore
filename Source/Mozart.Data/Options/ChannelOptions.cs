namespace Mozart.Options;

public class ChannelOptions
{
    public required int Id { get; init; }
    public int Capacity    { get; init; } = 100;
    public float Gem       { get; init; } = 1.0f;
    public float Exp       { get; init; } = 1.0f;


    public string MusicList { get; init; } = string.Empty;
    public string AlbumList { get; init; } = string.Empty;
    public string ItemData  { get; init; } = string.Empty;
    public bool? FreeMusic  { get; init; } = null;
}
