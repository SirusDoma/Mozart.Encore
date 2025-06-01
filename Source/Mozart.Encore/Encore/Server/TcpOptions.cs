using System.Net;

namespace Encore.Server;

public enum SessionMode
{
    Online    = 0,
    Emulation = 1
}

public class TcpOptions
{
    public const string Section = "Server";

    public string Address { get; set; } = "127.0.0.1";

    public int Port { get; set; }

    public int MaxConnections { get; set; }

    public int PacketBufferSize { get; set; }
}