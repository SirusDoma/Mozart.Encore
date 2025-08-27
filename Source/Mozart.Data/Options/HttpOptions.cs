namespace Mozart.Options;

public sealed class HttpOptions
{
    public const string Section = "Http";

    public bool Enabled { get; init; } = true;
    public string Address { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 15000;
}