using Microsoft.Extensions.Options;

using Encore.Server;

namespace Mozart.Workers.Gateway;

public interface IClientServer : ITcpServer<ClientSession>;

public class ClientServer : TcpServer<ClientSession>, IClientServer
{
    public ClientServer(IClientSessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}