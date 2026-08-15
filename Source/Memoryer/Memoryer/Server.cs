using Encore.Server;
using Encore.Server.Sessions;
using Microsoft.Extensions.Options;

namespace Memoryer;

public interface IMozartServer : ITcpServer<Session>;

public class GameServer : TcpServer<Session>, IMozartServer
{
    public GameServer(ISessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}
