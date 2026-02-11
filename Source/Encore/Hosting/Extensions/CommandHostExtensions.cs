using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Encore.Messaging;
using Encore.Server;

namespace Encore.Hosting.Extensions;

public static partial class CommandHostExtensions
{
    private interface ICommandHostBuilder
    {
        public IServiceCollection Services { get; }
    }

    private static void RebuildSingleton<TService>(IServiceCollection services,
        Func<IServiceProvider, Func<IServiceProvider, object>, TService> newFactory)
        where TService : class
    {
        EnsureDefaultServicesRegistered(services);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(TService));
        if (descriptor == null)
        {
            services.AddSingleton<TService>(p1 => newFactory(p1, p2 =>
                ActivatorUtilities.CreateInstance<TService>(p2)
            ));

            return;
        }

        var factory = descriptor.ImplementationFactory ?? (provider =>
            ActivatorUtilities.CreateInstance<TService>(provider));

        descriptor  = ServiceDescriptor.Singleton<TService>(provider => newFactory(provider, factory));
        services.Replace(descriptor);
    }

    private static void EnsureDefaultServicesRegistered(IServiceCollection services)
    {
        services.TryAddSingleton<DefaultMessageCodec>();
        services.AddSingleton<IMessageCodec, DefaultMessageCodec>(provider =>
            provider.GetRequiredService<DefaultMessageCodec>()
        );

        services.TryAddSingleton<CommandDispatcher>();
        if (services.All(d => d.ServiceType != typeof(ICommandDispatcher)))
        {
            services.AddSingleton<ICommandDispatcher, CommandDispatcher>(provider =>
                provider.GetRequiredService<CommandDispatcher>()
            );
        }
    }
}
