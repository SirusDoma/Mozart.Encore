using Encore.Server;
using Microsoft.Extensions.Options;

namespace Amadeus.Workers.Gateway;

public interface IClientServer : ITcpServer<ClientSession>;

public class ClientServer : TcpServer<ClientSession>, IClientServer
{
    public ClientServer(IClientSessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}
