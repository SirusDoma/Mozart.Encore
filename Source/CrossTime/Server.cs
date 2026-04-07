using Encore.Server;
using Microsoft.Extensions.Options;
using Mozart.Sessions;

namespace CrossTime;

public interface IMozartServer : ITcpServer<Session>;

public class MozartServer : TcpServer<Session>, IMozartServer
{
    public MozartServer(ISessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}
