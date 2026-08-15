using Encore.Server;
using Memoryer.Relay.Sessions;
using Microsoft.Extensions.Options;

namespace Memoryer.Relay;

public class TcpRelayServer : TcpServer<TcpRelaySession>
{
    public TcpRelayServer(IRelaySessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}

public interface ITcpRelayServerPool
{
    IReadOnlyList<TcpRelayServer> Servers { get; }
}

public class TcpRelayServerPool : ITcpRelayServerPool
{
    public TcpRelayServerPool(IRelaySessionFactory factory, IOptions<RelayOptions> options)
    {
        var relay = options.Value;
        Servers = relay.Endpoints
            .Select(endpoint => new TcpRelayServer(factory, Options.Create(new TcpOptions
            {
                Address          = endpoint.Address,
                Port             = endpoint.Port,
                MaxConnections   = relay.MaxConnections,
                PacketBufferSize = relay.PacketBufferSize
            })))
            .ToList();
    }

    public IReadOnlyList<TcpRelayServer> Servers { get; }
}

public class UdpRelayServer : UdpServer<UdpRelaySession>
{
    public UdpRelayServer(IUdpRelaySessionFactory factory, IOptions<UdpOptions> options)
        : base(factory, options)
    {
    }
}

public interface IUdpRelayServerPool
{
    IReadOnlyList<UdpRelayServer> Servers { get; }
}

public class UdpRelayServerPool : IUdpRelayServerPool
{
    public UdpRelayServerPool(IUdpRelaySessionFactory factory, IOptions<RelayOptions> options)
    {
        var relay = options.Value;
        Servers = relay.Endpoints
            .Select(endpoint => new UdpRelayServer(factory, Options.Create(new UdpOptions
            {
                Address           = endpoint.Address,
                Port              = endpoint.Port,
                ReceiveBufferSize = relay.PacketBufferSize
            })))
            .ToList();
    }

    public IReadOnlyList<UdpRelayServer> Servers { get; }
}
