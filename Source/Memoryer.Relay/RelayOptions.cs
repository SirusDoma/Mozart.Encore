using System.Net.Sockets;

namespace Memoryer.Relay;

public class RelayEndpoint
{
    public string Address { get; init; } = "127.0.0.1";
    public int Port       { get; init; }
}

public class RelayOptions
{
    public const string Section = "Relay";

    public bool Enabled                   { get; init; } = false;
    public bool P2PEnabled                { get; init; } = true;
    public int MaxConnections             { get; init; } = (int)SocketOptionName.MaxConnections;
    public int PacketBufferSize           { get; init; } = 4096;
    public IList<RelayEndpoint> Endpoints { get; init; } = [];
}
