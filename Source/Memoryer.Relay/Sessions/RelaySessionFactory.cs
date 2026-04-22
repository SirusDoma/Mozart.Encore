using System.Net.Sockets;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Memoryer.Relay.Sessions;

public interface IRelaySessionFactory : ISessionFactory<RelaySession>;

public class RelaySessionFactory(IServiceProvider provider) : IRelaySessionFactory
{
    public RelaySession CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<RelaySession>(provider, client);
    }
}
