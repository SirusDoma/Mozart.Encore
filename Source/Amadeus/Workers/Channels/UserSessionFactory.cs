using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Encore.Sessions;

namespace Amadeus.Workers.Channels;

public interface IUserSessionFactory : ISessionFactory<UserSession>;

public class UserSessionFactory : IUserSessionFactory
{
    private readonly IServiceProvider _provider;

    public UserSessionFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public UserSession CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<UserSession>(_provider, parameters.Prepend(client).ToArray());
    }
}
