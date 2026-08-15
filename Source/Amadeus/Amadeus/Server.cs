using Encore.Server;
using Encore.Server.Sessions;
using Microsoft.Extensions.Options;

namespace Amadeus;

public interface IMozartServer : ITcpServer<Session>;

public class MozartServer : TcpServer<Session>, IMozartServer
{
    public MozartServer(ISessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}
