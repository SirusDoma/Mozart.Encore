using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Encore.Server;
using Encore.Messaging;
using Encore.Sessions;

namespace Encore.Hosting.Extensions;

public static partial class CommandHostExtensions
{
    public interface IControllerMappingOptions
    {
        IControllerMappingOptions AddFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFilter>()
            where TFilter : class, ICommandFilter;

        IControllerMappingOptions AddFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFilter>(
            Func<IServiceProvider, TFilter> factory
        )
            where TFilter : class, ICommandFilter;

        IControllerMappingOptions AddFilter(ICommandFilter filter);
    }

    private class ControllerMappingOptions : IControllerMappingOptions
    {
        private readonly IServiceProvider _provider;
        private readonly List<ICommandFilter> _filters = [];

        public ControllerMappingOptions(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IControllerMappingOptions AddFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TFilter>()
            where TFilter : class, ICommandFilter
        {
            _filters.Add(ActivatorUtilities.CreateInstance<TFilter>(_provider));
            return this;
        }

        public IControllerMappingOptions AddFilter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
            TFilter>(Func<IServiceProvider, TFilter> factory)
            where TFilter : class, ICommandFilter
        {
            ArgumentNullException.ThrowIfNull(factory);
            _filters.Add(factory(_provider));
            return this;
        }

        public IControllerMappingOptions AddFilter(ICommandFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);
            _filters.Add(filter);
            return this;
        }

        public IEnumerable<ICommandFilter> GetFilters()
        {
            return _filters;
        }
    }

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
        provider.Services.TryAddSingleton<TCodec>();

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

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map<Session, TController>(session =>
            {
                using var scope = provider.CreateScope();
                return ActivatorUtilities.CreateInstance<TController>(scope.ServiceProvider, session);
            });

            return router;
        });

        return builder;
    }

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    public static ICommandRouteBuilder Map<TSession, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TController>(this ICommandRouteBuilder builder)
        where TSession    : Session
        where TController : CommandController
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map<TSession, TController>(session =>
            {
                using var scope = provider.CreateScope();
                return ActivatorUtilities.CreateInstance<TController>(scope.ServiceProvider, session);
            });

            return router;
        });

        return builder;
    }

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    public static ICommandRouteBuilder Map<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TController>(this ICommandRouteBuilder builder, Action<IControllerMappingOptions> configurer)
        where TController : CommandController
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
            {
                var router = (ICommandDispatcher)factory(provider);
                var options = new ControllerMappingOptions(provider);
                configurer(options);

                router.Map<Session, TController>(session =>
                {
                    using var scope = provider.CreateScope();
                    return ActivatorUtilities.CreateInstance<TController>(scope.ServiceProvider, session);
                }, options.GetFilters());

            return router;
        });

        return builder;
    }

    [RequiresUnreferencedCode("Controller registration require reflection to scan available handlers")]
    public static ICommandRouteBuilder Map<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        TController>(this ICommandRouteBuilder builder,
        Func<IServiceProvider, Session, TController> implementationFactory)
        where TController : CommandController
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.Map<Session, TController>(session =>
            {
                using var scope = provider.CreateScope();
                return implementationFactory(scope.ServiceProvider, session);
            });

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