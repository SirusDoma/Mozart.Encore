using Encore.Server;
using Microsoft.Extensions.Options;
using Mozart.Sessions;

namespace Memoryer;

public interface IMozartServer : ITcpServer<Session>;

public class GameServer : TcpServer<Session>, IMozartServer
{
    public GameServer(ISessionFactory factory, IOptions<TcpOptions> options)
        : base(factory, options)
    {
    }
}
