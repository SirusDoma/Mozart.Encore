using System.Net.Sockets;
using Encore.Server;
using Microsoft.Extensions.Options;

namespace Encore.Sessions;

public interface IUdpSessionFactory<out TSession>
    where TSession : IUdpSession
{
    TSession CreateSession(UdpClient transport, UdpReceiveResult received, params object[] parameters);
}

public interface IUdpSessionFactory : IUdpSessionFactory<UdpSession>
{
}

public sealed class UdpSessionFactory : IUdpSessionFactory
{
    private readonly UdpOptions _options;
    private readonly ICommandDispatcher _dispatcher;

    public UdpSessionFactory(IOptions<UdpOptions> options, ICommandDispatcher dispatcher)
    {
        _options    = options.Value;
        _dispatcher = dispatcher;
    }

    public UdpSession CreateSession(UdpClient transport, UdpReceiveResult received, params object[] parameters)
    {
        return new UdpSession(transport, _options, _dispatcher, received);
    }
}
