using System.Net.Sockets;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace Encore.Server.Sessions;

public interface ISessionFactory : ISessionFactory<Session>;

public class SessionFactory(IServiceProvider provider) : ISessionFactory
{
    public Session CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<Session>(provider, client);
    }
}
