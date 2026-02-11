using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;

using Encore.Server;
using Encore.Sessions;

using Mozart.Options;

namespace Mozart.Workers.Gateway;

public interface IGatewayServer : ITcpServer<ChannelSession>;

public class GatewayServer : TcpServer<ChannelSession>, IGatewayServer
{
    public GatewayServer(IChannelSessionFactory factory, IOptions<TcpOptions> tcpOptions, IOptions<GatewayOptions> options)
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
