using System.Net.Sockets;

namespace Encore.Server;

public class TcpOptions
{
    public const string Section = "Server";

    public string Address { get; init; } = "127.0.0.1";

    public int Port { get; init; }

    public int MaxConnections { get; init; } = (int)SocketOptionName.MaxConnections;

    public int PacketBufferSize { get; init; }
}