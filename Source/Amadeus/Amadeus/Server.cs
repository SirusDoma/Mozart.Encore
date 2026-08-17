using Encore.Server;
using Encore.Server.Sessions;
using Microsoft.Extensions.Options;

namespace Amadeus;

public interface IMozartServer : ITcpServer<Session>;

public class AmadeusServer : TcpServer<Session>, IMozartServer
{
    public AmadeusServer(ISessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}
