namespace Mozart.Options;

public enum SessionDriver
{
    Memory,
    Distributed
}

public class GatewayOptions
{
    public const string Section = "Gateway";

    public int Id         { get; init; } = 0;
    public string Address { get; init; } = string.Empty;
    public int Port       { get; init; }
    public int Timeout    { get; init; } = 30;

    public IList<ChannelOptions> Channels { get; init; } = [];
}