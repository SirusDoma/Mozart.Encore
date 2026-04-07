using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Mozart.Sessions;

public interface ISessionFactory : Encore.Sessions.ISessionFactory<Session>;

public class SessionFactory(IServiceProvider provider) : ISessionFactory
{
    public Session CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<Session>(provider, client);
    }
}
