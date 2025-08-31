using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Encore.Sessions;

namespace Amadeus.Workers.Gateway;

public interface IChannelSessionFactory : ISessionFactory<ChannelSession>;

public class ChannelSessionFactory : IChannelSessionFactory
{
    private readonly IServiceProvider _provider;

    public ChannelSessionFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ChannelSession CreateSession(TcpClient client, params object[] parameters)
    {
        return ActivatorUtilities.CreateInstance<ChannelSession>(_provider, parameters.Prepend(client).ToArray());
    }
} 