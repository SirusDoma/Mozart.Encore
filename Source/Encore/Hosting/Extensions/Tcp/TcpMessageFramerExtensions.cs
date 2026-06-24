using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Encore.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Encore.Hosting.Extensions;

public static class TcpMessageFramerExtensions
{
    public interface ITcpMessageFramerFactoryBuilder
    {
        public IServiceCollection Services { get; }
    }

    private class TcpMessageFramerFactoryBuilder : ITcpMessageFramerFactoryBuilder
    {
        public TcpMessageFramerFactoryBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }

    public static ITcpMessageFramerFactoryBuilder AddFramerFactory<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TFramer
    >(this ITcpMessageFramerFactoryBuilder builder)
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

    public static IHostBuilder ConfigureTcpFramer(this IHostBuilder builder, Action<ITcpMessageFramerFactoryBuilder> configure)
    {
        return ConfigureTcpFramer(builder, (_, framer) => configure(framer));
    }

    public static IHostBuilder ConfigureTcpFramer(this IHostBuilder builder, Action<HostBuilderContext, ITcpMessageFramerFactoryBuilder> configure)
    {
        return builder.ConfigureServices((context, services) =>
        {
            configure(context, new TcpMessageFramerFactoryBuilder(services));
        });
    }
}
