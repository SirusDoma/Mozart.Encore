using System.Diagnostics.CodeAnalysis;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Encore.Hosting.Extensions;

public static class TcpSessionExtensions
{
    public interface ITcpSessionProvider
    {
        IServiceCollection Services { get; }
    }

    public interface ITcpSessionProvider<TSession> : ITcpSessionProvider
        where TSession : ITcpSession
    {
        ITcpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >() where TSessionFactory : class, ISessionFactory<TSession>;

        ITcpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >(Func<IServiceProvider, TSessionFactory> factory)
            where TSessionFactory : class, ISessionFactory<TSession>;

        ITcpSessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionManager
        >() where TSessionManager : class, ISessionManager<TSession>;

        ITcpSessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionManager
        >(Func<IServiceProvider, TSessionManager> factory)
            where TSessionManager : class, ISessionManager<TSession>;
    }

    public class TcpSessionProvider : ITcpSessionProvider
    {
        public IServiceCollection Services { get; }

        public TcpSessionProvider(IServiceCollection services)
        {
            Services = services;
        }
    }

    public class TcpSessionProvider<TSession> : ITcpSessionProvider<TSession>
        where TSession : ITcpSession
    {
        public TcpSessionProvider(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public ITcpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >() where TSessionFactory : class, ISessionFactory<TSession>
        {
            Services.AddSingleton<TSessionFactory>();

            Services.AddSingleton<ISessionFactory<TSession>>(provider =>
                provider.GetRequiredService<TSessionFactory>()
            );

            if (typeof(TSession) == typeof(TcpSession))
            {
                Services.AddSingleton<ISessionFactory>(provider =>
                    (ISessionFactory)provider.GetRequiredService<ISessionFactory<TSession>>()
                );
            }

            return this;
        }

        public ITcpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >(Func<IServiceProvider, TSessionFactory> factory)
            where TSessionFactory : class, ISessionFactory<TSession>
        {
            Services.AddSingleton(factory);

            Services.AddSingleton<ISessionFactory<TSession>>(provider =>
                provider.GetRequiredService<TSessionFactory>()
            );

            if (typeof(TSession) == typeof(TcpSession))
            {
                Services.AddSingleton<ISessionFactory>(provider =>
                    (ISessionFactory)provider.GetRequiredService<ISessionFactory<TSession>>()
                );
            }

            return this;
        }

        public ITcpSessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSessionManager
        >() where TSessionManager : class, ISessionManager<TSession>
        {
            Services.AddSingleton<TSessionManager>();

            Services.AddSingleton<ISessionManager<TSession>>(provider =>
                provider.GetRequiredService<TSessionManager>()
            );

            if (typeof(TSession) == typeof(TcpSession))
            {
                Services.AddSingleton<ISessionManager>(provider =>
                    (ISessionManager)provider.GetRequiredService<ISessionManager<TSession>>()
                );
            }

            return this;
        }

        public ITcpSessionProvider<TSession> AddManager<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSessionManager
        >(Func<IServiceProvider, TSessionManager> factory)
            where TSessionManager : class, ISessionManager<TSession>
        {
            Services.AddSingleton(factory);

            Services.AddSingleton<ISessionManager<TSession>>(provider =>
                provider.GetRequiredService<TSessionManager>()
            );

            if (typeof(TSession) == typeof(TcpSession))
            {
                Services.AddSingleton<ISessionManager>(provider =>
                    (ISessionManager)provider.GetRequiredService<ISessionManager<TSession>>()
                );
            }

            return this;
        }
    }

    public static ITcpSessionProvider<TSession> UseTcpSession<TSession>(this ITcpSessionProvider provider)
        where TSession : ITcpSession
    {
        return new TcpSessionProvider<TSession>(provider.Services);
    }

    public static IHostBuilder ConfigureTcpSessions(this IHostBuilder builder, Action<ITcpSessionProvider> configure)
    {
        return ConfigureTcpSessions(builder, (_, provider) => configure(provider));
    }

    public static IHostBuilder ConfigureTcpSessions(this IHostBuilder builder, Action<HostBuilderContext, ITcpSessionProvider> configure)
    {
        return builder.ConfigureServices((context, services) =>
        {
            configure(context, new TcpSessionProvider(services));
        });
    }
}
