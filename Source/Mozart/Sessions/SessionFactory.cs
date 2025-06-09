using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;

namespace Mozart.Sessions;

public class SessionFactory(IServiceProvider provider) : Encore.Sessions.ISessionFactory<Session>
{
    public Session CreateSession(TcpClient client)
    {
        return ActivatorUtilities.CreateInstance<Session>(provider, client);
    }
}