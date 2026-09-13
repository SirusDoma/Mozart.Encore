using System.Net.Sockets;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Encore.Workers.Channels;

public interface IGatewaySessionFactory : ISessionFactory<GatewaySession>;

public class GatewaySessionFactory(IServiceProvider provider) : IGatewaySessionFactory
{
    public GatewaySession CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<GatewaySession>(provider, parameters.Prepend(client).ToArray());
    }
}
