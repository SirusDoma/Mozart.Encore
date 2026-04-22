using System.Diagnostics.CodeAnalysis;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Encore.Hosting.Extensions;

public static class UdpSessionExtensions
{
    public interface IUdpSessionProvider
    {
        IServiceCollection Services { get; }
    }

    public interface IUdpSessionProvider<TSession> : IUdpSessionProvider
        where TSession : class, IUdpSession
    {
        IUdpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >() where TSessionFactory : class, IUdpSessionFactory<TSession>;

        IUdpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >(Func<IServiceProvider, TSessionFactory> factory)
            where TSessionFactory : class, IUdpSessionFactory<TSession>;
    }

    public class UdpSessionProvider : IUdpSessionProvider
    {
        public IServiceCollection Services { get; }

        public UdpSessionProvider(IServiceCollection services)
        {
            Services = services;
        }
    }

    public class UdpSessionProvider<TSession> : IUdpSessionProvider<TSession>
        where TSession : class, IUdpSession
    {
        public UdpSessionProvider(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public IUdpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >() where TSessionFactory : class, IUdpSessionFactory<TSession>
        {
            Services.AddSingleton<TSessionFactory>();

            Services.AddSingleton<IUdpSessionFactory<TSession>>(provider =>
                provider.GetRequiredService<TSessionFactory>()
            );

            if (typeof(TSession) == typeof(UdpSession))
            {
                Services.AddSingleton<IUdpSessionFactory>(provider =>
                    (IUdpSessionFactory)provider.GetRequiredService<IUdpSessionFactory<TSession>>()
                );
            }

            return this;
        }

        public IUdpSessionProvider<TSession> AddFactory<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TSessionFactory
        >(Func<IServiceProvider, TSessionFactory> factory)
            where TSessionFactory : class, IUdpSessionFactory<TSession>
        {
            Services.AddSingleton(factory);

            Services.AddSingleton<IUdpSessionFactory<TSession>>(provider =>
                provider.GetRequiredService<TSessionFactory>()
            );

            if (typeof(TSession) == typeof(UdpSession))
            {
                Services.AddSingleton<IUdpSessionFactory>(provider =>
                    (IUdpSessionFactory)provider.GetRequiredService<IUdpSessionFactory<TSession>>()
                );
            }

            return this;
        }
    }

    public static IUdpSessionProvider<TSession> UseUdpSession<TSession>(this IUdpSessionProvider provider)
        where TSession : class, IUdpSession
    {
        return new UdpSessionProvider<TSession>(provider.Services);
    }

    public static IHostBuilder ConfigureUdpSessions(this IHostBuilder builder, Action<IUdpSessionProvider> configure)
    {
        return ConfigureUdpSessions(builder, (_, provider) => configure(provider));
    }

    public static IHostBuilder ConfigureUdpSessions(this IHostBuilder builder,
        Action<HostBuilderContext, IUdpSessionProvider> configure)
    {
        return builder.ConfigureServices((context, services) =>
        {
            configure(context, new UdpSessionProvider(services));
        });
    }
}
