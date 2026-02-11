using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Encore.Sessions;

namespace Encore.Hosting.Extensions;

public static class SessionExtensions
{
    public interface ISessionProvider
    {
        IServiceCollection Services { get; }
    }

    public interface ISessionProvider<TSession> : ISessionProvider
        where TSession : Session
    {
        ISessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >() where TSessionFactory : class, ISessionFactory<TSession>;

        ISessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >(Func<IServiceProvider, TSessionFactory> factory)
            where TSessionFactory : class, ISessionFactory<TSession>;

        ISessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionManager
        >() where TSessionManager : class, ISessionManager<TSession>;

        ISessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionManager
        >(Func<IServiceProvider, TSessionManager> factory)
            where TSessionManager : class, ISessionManager<TSession>;
    }

    public class SessionProvider : ISessionProvider
    {
        public IServiceCollection Services { get; }

        public SessionProvider(IServiceCollection services)
        {
            Services = services;
        }
    }

    public class SessionProvider<TSession> : ISessionProvider<TSession>
        where TSession : Session
    {
        public SessionProvider(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public ISessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >() where TSessionFactory : class, ISessionFactory<TSession>
        {
            Services.AddSingleton<TSessionFactory>();

            Services.AddSingleton<ISessionFactory<TSession>>(provider =>
                provider.GetRequiredService<TSessionFactory>()
            );

            if (typeof(TSession) == typeof(Session))
            {
                Services.AddSingleton<ISessionFactory>(provider =>
                    (ISessionFactory)provider.GetRequiredService<ISessionFactory<TSession>>()
                );
            }

            return this;
        }

        public ISessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >(Func<IServiceProvider, TSessionFactory> factory)
            where TSessionFactory : class, ISessionFactory<TSession>
        {
            Services.AddSingleton(factory);

            Services.AddSingleton<ISessionFactory<TSession>>(provider =>
                provider.GetRequiredService<TSessionFactory>()
            );

            if (typeof(TSession) == typeof(Session))
            {
                Services.AddSingleton<ISessionFactory>(provider =>
                    (ISessionFactory)provider.GetRequiredService<ISessionFactory<TSession>>()
                );
            }

            return this;
        }

        public ISessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSessionManager
        >() where TSessionManager : class, ISessionManager<TSession>
        {
            Services.AddSingleton<TSessionManager>();

            Services.AddSingleton<ISessionManager<TSession>>(provider =>
                provider.GetRequiredService<TSessionManager>()
            );

            if (typeof(TSession) == typeof(Session))
            {
                Services.AddSingleton<ISessionManager>(provider =>
                    (ISessionManager)provider.GetRequiredService<ISessionManager<TSession>>()
                );
            }

            return this;
        }

        public ISessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSessionManager
        >(Func<IServiceProvider, TSessionManager> factory)
            where TSessionManager : class, ISessionManager<TSession>
        {
            Services.AddSingleton(factory);

            Services.AddSingleton<ISessionManager<TSession>>(provider =>
                provider.GetRequiredService<TSessionManager>()
            );

            if (typeof(TSession) == typeof(Session))
            {
                Services.AddSingleton<ISessionManager>(provider =>
                    (ISessionManager)provider.GetRequiredService<ISessionManager<TSession>>()
                );
            }

            return this;
        }
    }

    public static ISessionProvider<TSession> UseSession<TSession>(this ISessionProvider provider)
        where TSession : Session
    {
        return new SessionProvider<TSession>(provider.Services);
    }

    public static IHostBuilder ConfigureSessions(this IHostBuilder builder, Action<ISessionProvider> configure)
    {
        return ConfigureSessions(builder, (_, provider) => configure(provider));
    }

    public static IHostBuilder ConfigureSessions(this IHostBuilder builder, Action<HostBuilderContext, ISessionProvider> configure)
    {
        return builder.ConfigureServices((context, services) =>
        {
            configure(context, new SessionProvider(services));
        });
    }
}
