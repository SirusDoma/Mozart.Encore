using Encore.Server;
using Encore.Server.Sessions;
using Microsoft.Extensions.Options;
using Mozart.Options;

namespace Encore.Workers.Gateway;

public interface IGatewayServer : ITcpServer<Session>;

public class GatewayServer : TcpServer<Session>, IGatewayServer
{
    public GatewayServer(ISessionFactory factory, IOptions<TcpOptions> tcpOptions, IOptions<GatewayOptions> options)
        : base(factory, Microsoft.Extensions.Options.Options.Create(
            new TcpOptions
            {
                Address          = options.Value.Address,
                Port             = options.Value.Port,
                PacketBufferSize = tcpOptions.Value.PacketBufferSize
            }))
    {
    }
}
