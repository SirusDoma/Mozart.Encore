namespace Mozart.Options;

public enum SessionDriver
{
    Memory,
    Distributed
}

public class GatewayOptions
{
    public const string Section = "Gateway";

    public int Id { get; init; } = 0;

    public IList<ChannelOptions> Channels { get; init; } = [];
}