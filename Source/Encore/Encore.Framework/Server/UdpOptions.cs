namespace Encore.Server;

public class UdpOptions
{
    public const string Section = "UdpServer";

    public string Address { get; init; } = "127.0.0.1";

    public int Port { get; init; }

    public int ReceiveBufferSize { get; init; } = 0;
}
