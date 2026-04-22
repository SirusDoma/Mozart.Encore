using System.Net.Sockets;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Memoryer.Relay.Sessions;

public interface IUdpRelaySessionFactory : IUdpSessionFactory<UdpRelaySession>;

public class UdpRelaySessionFactory(IServiceProvider provider) : IUdpRelaySessionFactory
{
    public UdpRelaySession CreateSession(UdpClient transport, UdpReceiveResult received, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<UdpRelaySession>(provider, transport, received);
    }
}
