using System.Net.Sockets;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Memoryer.Relay.Sessions;

public interface IRelaySessionFactory : ISessionFactory<TcpRelaySession>;

public class RelaySessionFactory(IServiceProvider provider) : IRelaySessionFactory
{
    public TcpRelaySession CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<TcpRelaySession>(provider, client);
    }
}
