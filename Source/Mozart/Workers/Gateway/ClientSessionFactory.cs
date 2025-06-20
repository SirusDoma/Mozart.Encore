using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Encore.Sessions;

namespace Mozart.Workers.Gateway;

public interface IClientSessionFactory : ISessionFactory<ClientSession>;

public class ClientSessionFactory : IClientSessionFactory
{
    private readonly IServiceProvider _provider;

    public ClientSessionFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ClientSession CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<ClientSession>(_provider, parameters.Prepend(client).ToArray());
    }
} 