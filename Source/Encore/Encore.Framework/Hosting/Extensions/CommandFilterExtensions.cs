using System.Diagnostics.CodeAnalysis;
using Encore.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Encore.Hosting.Extensions;

public static partial class CommandHostExtensions
{
    public interface ICommandDispatcherFilterBuilder
    {
        public IServiceCollection Services { get; }
    }

    private class CommandDispatcherFilterBuilder : ICommandDispatcherFilterBuilder
    {
        public CommandDispatcherFilterBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    public static ICommandDispatcherFilterBuilder AddFilter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommandFilter
    >(this ICommandDispatcherFilterBuilder builder)
        where TCommandFilter : class, ICommandFilter
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.AddFilter(ActivatorUtilities.CreateInstance<TCommandFilter>(provider));

            return router;
        });

        return builder;
    }

    public static ICommandDispatcherFilterBuilder AddFilter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommandFilter
    >(this ICommandDispatcherFilterBuilder builder, Func<IServiceProvider, TCommandFilter> implementationFactory)
        where TCommandFilter : class, ICommandFilter
    {
        EnsureDefaultServicesRegistered(builder.Services);

        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.AddFilter(implementationFactory(provider));

            return router;
        });

        return builder;
    }

    public static ICommandDispatcherFilterBuilder AddExceptionHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TExceptionHandler
    >(this ICommandDispatcherFilterBuilder builder)
        where TExceptionHandler : class, ICommandExceptionHandler
    {
        EnsureDefaultServicesRegistered(builder.Services);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(ICommandExceptionHandler)))
            throw new InvalidOperationException("An exception filter has already been added");

        builder.Services.AddSingleton<ICommandExceptionHandler, TExceptionHandler>();
        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.AddExceptionFilter(provider.GetRequiredService<ICommandExceptionHandler>());

            return router;
        });

        return builder;
    }

    public static ICommandDispatcherFilterBuilder AddExceptionHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TExceptionHandler
    >(this ICommandDispatcherFilterBuilder builder, Func<IServiceProvider, TExceptionHandler> implementationFactory)
        where TExceptionHandler : class, ICommandExceptionHandler
    {
        EnsureDefaultServicesRegistered(builder.Services);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(ICommandExceptionHandler)))
            throw new InvalidOperationException("An exception filter has already been added");

        builder.Services.AddSingleton<ICommandExceptionHandler, TExceptionHandler>(implementationFactory);
        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.AddExceptionFilter(provider.GetRequiredService<ICommandExceptionHandler>());

            return router;
        });

        return builder;
    }


    public static ICommandDispatcherFilterBuilder AddExceptionLogger<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TExceptionLogger
    >(this ICommandDispatcherFilterBuilder builder)
        where TExceptionLogger : class, ICommandExceptionLogger
    {
        EnsureDefaultServicesRegistered(builder.Services);

        if (builder.Services.Any(descriptor => descriptor.ServiceType == typeof(ICommandExceptionLogger)))
            throw new InvalidOperationException("An exception filter has already been added");

        builder.Services.AddTransient<ICommandExceptionLogger, TExceptionLogger>();
        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.AddExceptionLogger(provider.GetRequiredService<ICommandExceptionLogger>());

            return router;
        });

        return builder;
    }

    public static ICommandDispatcherFilterBuilder AddExceptionLogger<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TExceptionLogger
    >(this ICommandDispatcherFilterBuilder builder, Func<IServiceProvider, TExceptionLogger> implementationFactory)
        where TExceptionLogger : class, ICommandExceptionLogger
    {
        EnsureDefaultServicesRegistered(builder.Services);


        builder.Services.AddTransient<ICommandExceptionLogger, TExceptionLogger>(implementationFactory);
        RebuildSingleton(builder.Services, (provider, factory) =>
        {
            var router = (ICommandDispatcher)factory(provider);
            router.AddExceptionLogger(provider.GetRequiredService<ICommandExceptionLogger>());

            return router;
        });

        return builder;
    }

    public static IHostBuilder ConfigureFilters(this IHostBuilder builder,
        Action<ICommandDispatcherFilterBuilder> configure)
    {
        return ConfigureFilters(builder, (_, filterBuilder) => configure(filterBuilder));
    }

    public static IHostBuilder ConfigureFilters(this IHostBuilder builder,
        Action<HostBuilderContext, ICommandDispatcherFilterBuilder> configure)
    {
        return builder.ConfigureServices((context, services) =>
        {
            configure(context, new CommandDispatcherFilterBuilder(services));
        });
    }
}
