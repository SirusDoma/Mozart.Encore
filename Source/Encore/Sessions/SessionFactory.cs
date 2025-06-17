using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Encore.Messaging;
using Encore.Server;

namespace Encore.Sessions;

public interface ISessionFactory<out TSession>
    where TSession : Session
{
    TSession CreateSession(TcpClient client);
}

public interface ISessionFactory : ISessionFactory<Session>
{
}

public sealed class SessionFactory : ISessionFactory
{
    private readonly IMessageFramerFactory _framerFactory;
    private readonly ICommandDispatcher _dispatcher;
    private readonly TcpOptions _options;

    public SessionFactory(IOptions<TcpOptions> options)
    {
        _options       = options.Value;
        _framerFactory = new SizePrefixedMessageFramerFactory<ushort>();
        _dispatcher    = new CommandDispatcher();
    }

    public SessionFactory(
        IOptions<TcpOptions> options,
        IMessageFramerFactory? framerFactory = null,
        ICommandDispatcher? dispatcher       = null
    )
    {
        _options       = options.Value;
        _framerFactory = framerFactory ?? new SizePrefixedMessageFramerFactory<ushort>();
        _dispatcher    = dispatcher    ?? new CommandDispatcher();
    }

    public Session CreateSession(TcpClient client)
    {
        return new Session(client, _options, _framerFactory, _dispatcher);
    }
}