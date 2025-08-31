using Microsoft.Extensions.Options;

using Encore.Server;
using Mozart.Sessions;

namespace Amadeus;

public interface IMozartServer : ITcpServer<Session>;

public class MozartServer : TcpServer<Session>, IMozartServer
{
    public MozartServer(ISessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}