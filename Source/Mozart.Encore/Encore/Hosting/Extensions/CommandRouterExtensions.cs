using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Encore.Server;
using Encore.Messaging;
using Encore.Sessions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Encore.Hosting.Extensions;

public static partial class CommandHostExtensions
{

    public interface ICommandRouteBuilder
    {
        IServiceCollection Services { get; }
    }

    public interface ICommandRouteProvider : ICommandRouteBuilder
    {
    }

    private class CommandRouteProvider : ICommandRouteProvider
    {
        public CommandRouteProvider(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    public static ICommandRouteBuilder UseCodec<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCodec
    >(this ICommandRouteProvider provider)
        where TCodec : class, IMessageCodec
    {
        RebuildSingleton<TCodec>(provider.Services, (pvd, factory) =>
            (TCodec)factory(pvd));

        RebuildSingleton<IMessageCodec>(provider.Services, (pvd, _) => pvd.GetRequiredService<TCodec>());

        provider.Services.AddSingleton<IMessageCodec, TCodec>(pvd => pvd.GetRequiredService<TCodec>());

        return provider;
    }

    public static ICommandRouteBuilder Map<
        TSession,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponse
    >(this ICommandRouteBuilder builder, Func<TSession, TRequest, CancellationToken, Task<TResponse>> handler)
        where TSession  : Session
        where TRequest  : class, IMessage
        where TResponse : class, IMessage
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map(handler);

            return router;
        });

        return builder;
    }

    public static ICommandRouteBuilder Map<TSession,[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TRequest>(this ICommandRouteBuilder builder, Func<TSession, TRequest, Task> handler)
        where TSession : Session
        where TRequest : class, IMessage
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map(handler);

            return router;
        });

        return builder;
    }

    public static ICommandRouteBuilder Map<TSession, TCommand>(this ICommandRouteBuilder builder,
        TCommand command, Func<TSession, Task> handler)
        where TSession : Session
        where TCommand : Enum
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map(command, handler);

            return router;
        });

        return builder;
    }

    public static ICommandRouteBuilder Map<TSession, TCommand>(this ICommandRouteBuilder builder,
        TCommand command, Func<TSession, CancellationToken, Task> handler)
        where TSession : Session
        where TCommand : Enum
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map(command, handler);

            return router;
        });

        return builder;
    }

    public static ICommandRouteBuilder Map<TSession,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRequest,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TResponse
    >(this ICommandRouteBuilder builder, Func<TSession, TRequest, Task<TResponse>> handler)
        where TSession  : Session
        where TRequest  : class, IMessage
        where TResponse : class, IMessage
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map(handler);

            return router;
        });

        return builder;
    }

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    public static ICommandRouteBuilder Map<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TController>(this ICommandRouteBuilder builder)
        where TController : CommandController
    {
        EnsureDefaultServicesRegistered(builder.Services);

        builder.Services.AddTransient<TController>();
        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map<Session, TController>(session =>
                ActivatorUtilities.CreateInstance<TController>(provider, session)
            );

            return router;
        });

        return builder;
    }

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    public static ICommandRouteBuilder Map<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TController>(this ICommandRouteBuilder builder,
        Func<IServiceProvider, TController> implementationFactory)
        where TController : CommandController
    {
        EnsureDefaultServicesRegistered(builder.Services);

        builder.Services.AddTransient(implementationFactory);
        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map<Session, TController>(session =>
                ActivatorUtilities.CreateInstance<TController>(provider, session)
            );

            return router;
        });

        return builder;
    }

    public static IHostBuilder ConfigureRoutes(this IHostBuilder builder, Action<ICommandRouteProvider> configure)
    {
        return ConfigureRoutes(builder, (_, provider) => configure(provider));
    }

    public static IHostBuilder ConfigureRoutes(this IHostBuilder builder, Action<HostBuilderContext, ICommandRouteProvider> configure)
    {
        return builder.ConfigureServices((context, services) =>
        {
            configure(context, new CommandRouteProvider(services));
        });
    }
}