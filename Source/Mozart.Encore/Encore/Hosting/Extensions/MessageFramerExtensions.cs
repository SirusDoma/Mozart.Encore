
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Encore.Messaging;

namespace Encore.Hosting.Extensions;

public static class MessageFramerExtensions
{
    public interface IMessageFramerFactoryBuilder
    {
        public IServiceCollection Services { get; }
    }

    private class MessageFramerFactoryBuilder : IMessageFramerFactoryBuilder
    {
        public MessageFramerFactoryBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    public static IMessageFramerFactoryBuilder AddFramerFactory<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFramer
    >(this IMessageFramerFactoryBuilder builder)
        where TFramer : class, IMessageFramer
    {
        builder.Services.AddSingleton<IMessageFramerFactory>(provider =>
            new MessageFramerFactory<TFramer>(provider)
        );

        return builder;
    }

    private class MessageFramerFactory<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFramer
    > : IMessageFramerFactory
        where TFramer : class, IMessageFramer
    {
        private readonly IServiceProvider _provider;

        public MessageFramerFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        private IMessageFramer CreateFramer(NetworkStream stream, IServiceProvider provider)
        {
            return ActivatorUtilities.CreateInstance<TFramer>(provider, stream);
        }

        IMessageFramer IMessageFramerFactory.CreateFramer(NetworkStream stream)
        {
            return CreateFramer(stream, _provider);
        }
    }

    public static IHostBuilder ConfigureFramer(this IHostBuilder builder, Action<IMessageFramerFactoryBuilder> configure)
    {
        return ConfigureFramer(builder, (_, framer) => configure(framer));
    }

    public static IHostBuilder ConfigureFramer(this IHostBuilder builder, Action<HostBuilderContext, IMessageFramerFactoryBuilder> configure)
    {
        return builder.ConfigureServices((context, services) =>
        {
            configure(context, new MessageFramerFactoryBuilder(services));
        });
    }
}