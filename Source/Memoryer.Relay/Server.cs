using Encore.Server;
using Memoryer.Relay.Sessions;
using Microsoft.Extensions.Options;

namespace Memoryer.Relay;

public class RelayServer : TcpServer<RelaySession>
{
    public RelayServer(IRelaySessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}

public interface IRelayServerPool
{
    IReadOnlyList<RelayServer> Servers { get; }
}

public class RelayServerPool : IRelayServerPool
{
    public RelayServerPool(IRelaySessionFactory factory, IOptions<RelayOptions> options)
    {
        var relay = options.Value;
        Servers = relay.Endpoints
            .Select(endpoint => new RelayServer(factory, Options.Create(new TcpOptions
            {
                Address          = endpoint.Address,
                Port             = endpoint.Port,
                MaxConnections   = relay.MaxConnections,
                PacketBufferSize = relay.PacketBufferSize
            })))
            .ToList();
    }

    public IReadOnlyList<RelayServer> Servers { get; }
}
