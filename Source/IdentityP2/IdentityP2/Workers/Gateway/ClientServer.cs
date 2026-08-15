using Encore.Server;
using Microsoft.Extensions.Options;

namespace Identity.Workers.Gateway;

public interface IClientServer : ITcpServer<ClientSession>;

public class ClientServer : TcpServer<ClientSession>, IClientServer
{
    public ClientServer(IClientSessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}
