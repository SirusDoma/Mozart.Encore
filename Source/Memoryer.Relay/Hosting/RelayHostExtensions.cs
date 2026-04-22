using Encore.Sessions;
using Memoryer.Relay.Services;
using Memoryer.Relay.Sessions;
using Memoryer.Relay.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace Memoryer.Relay.Hosting;

public static class RelayHostExtensions
{
    public static IServiceCollection AddTcpRelayServices(this IServiceCollection services)
    {
        // TCP relay sessions
        services.AddSingleton<RelaySessionFactory>();
        services.AddSingleton<IRelaySessionFactory>(p => p.GetRequiredService<RelaySessionFactory>());
        services.AddSingleton<ISessionFactory<TcpRelaySession>>(p => p.GetRequiredService<IRelaySessionFactory>());

        services.AddSingleton<TcpRelaySessionManager>();
        services.AddSingleton<ITcpRelaySessionManager>(p => p.GetRequiredService<TcpRelaySessionManager>());
        services.AddSingleton<ISessionManager<TcpRelaySession>>(p => p.GetRequiredService<ITcpRelaySessionManager>());

        services.AddSingleton<ITcpRelayServerPool, TcpRelayServerPool>();
        services.AddHostedService<TcpRelayWorker>();

        // Shared services
        services.AddSingleton<IGameSessionService, GameSessionService>();
        services.AddSingleton<IRelaySessionLocator, RelaySessionLocator>();

        return services;
    }

    public static IServiceCollection AddUdpRelayServices(this IServiceCollection services)
    {
        // UDP relay sessions
        services.AddSingleton<UdpRelayPeerRegistry>();
        services.AddSingleton<IUdpRelayPeerRegistry>(p => p.GetRequiredService<UdpRelayPeerRegistry>());

        services.AddSingleton<UdpRelaySessionFactory>();
        services.AddSingleton<IUdpRelaySessionFactory>(p => p.GetRequiredService<UdpRelaySessionFactory>());
        services.AddSingleton<IUdpSessionFactory<UdpRelaySession>>(p => p.GetRequiredService<IUdpRelaySessionFactory>());

        services.AddSingleton<UdpSessionManager<UdpRelaySession>>();
        services.AddSingleton<IUdpSessionManager<UdpRelaySession>>(p =>
            p.GetRequiredService<UdpSessionManager<UdpRelaySession>>());
        services.AddSingleton<ISessionManager<UdpRelaySession>>(p =>
            p.GetRequiredService<UdpSessionManager<UdpRelaySession>>());

        services.AddSingleton<IUdpRelayServerPool, UdpRelayServerPool>();
        services.AddHostedService<UdpRelayWorker>();

        return services;
    }
}
