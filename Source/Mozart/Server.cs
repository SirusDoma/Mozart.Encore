using Microsoft.Extensions.Options;

using Encore.Server;
using Mozart.Sessions;

namespace Mozart;

public interface IMozartServer : ITcpServer<Session>;

public class MozartServer : TcpServer<Session>, IMozartServer
{
    public MozartServer(Encore.Sessions.ISessionFactory<Session> factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}