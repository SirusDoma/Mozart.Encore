using Encore.Sessions;
using Memoryer.Relay.Services;
using Memoryer.Relay.Sessions;
using Memoryer.Relay.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace Memoryer.Relay.Hosting;

public static class RelayHostExtensions
{
    public static IServiceCollection AddRelayServices(this IServiceCollection services)
    {
        services.AddSingleton<RelaySessionFactory>();
        services.AddSingleton<IRelaySessionFactory>(p => p.GetRequiredService<RelaySessionFactory>());
        services.AddSingleton<ISessionFactory<RelaySession>>(p => p.GetRequiredService<IRelaySessionFactory>());

        services.AddSingleton<RelaySessionManager>();
        services.AddSingleton<IRelaySessionManager>(p => p.GetRequiredService<RelaySessionManager>());
        services.AddSingleton<ISessionManager<RelaySession>>(p => p.GetRequiredService<IRelaySessionManager>());

        services.AddSingleton<IGameSessionService, GameSessionService>();

        services.AddSingleton<IRelayServerPool, RelayServerPool>();
        services.AddHostedService<RelayWorker>();
        return services;
    }
}
