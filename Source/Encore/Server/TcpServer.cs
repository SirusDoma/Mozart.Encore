using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Encore.Sessions;
using Encore.Messaging;

namespace Encore.Server;

public interface ITcpServer<TSession> : IDisposable
    where TSession : Session
{
    public Socket Socket { get; }

    public bool Active { get; }

    public TcpOptions Options { get; }

    void Start(int maxConnections = (int)SocketOptionName.MaxConnections);

    Task<TSession> AcceptSession(CancellationToken cancellationToken);
}

public interface ITcpServer : ITcpServer<Session>
{
}

public class TcpServer : TcpServer<Session>, ITcpServer
{
    public TcpServer(
        IOptions<TcpOptions>   options,
        SessionFactory?        sessionFactory = null,
        IMessageFramerFactory? framerFactory  = null,
        ICommandDispatcher?    dispatcher     = null
    )
        : base(sessionFactory ?? new SessionFactory(options, framerFactory, dispatcher), options)
    {
    }
}

public class TcpServer<TSession> : ITcpServer<TSession>
    where TSession : Session
{
    private readonly TcpListener _listener;
    private readonly ISessionFactory<TSession> _factory;

    public TcpServer(ISessionFactory<TSession> factory, IOptions<TcpOptions> options)
    {
        _listener = new TcpListener(IPAddress.Parse(options.Value.Address), options.Value.Port);
        _factory  = factory;

        Options   = options.Value;
    }

    public Socket Socket => _listener.Server;

    public bool Active { get; private set; } = false;

    public TcpOptions Options { get; init; }

    public async Task<TSession> AcceptSession(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && Active)
        {
            var client  = await _listener.AcceptTcpClientAsync(cancellationToken);
            var session = _factory.CreateSession(client);

            if (!Active)
                break;

            return session;
        }

        return await Task.FromCanceled<TSession>(cancellationToken);
    }

    public void Start(int maxConnections = (int)SocketOptionName.MaxConnections)
    {
        if (Active)
            return;

        _listener.Start(maxConnections);

        Active = true;
    }

    public Task Stop()
    {
        if (!Active)
            return Task.CompletedTask;

        Active = false;
        _listener.Stop();

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Stop();
        _listener.Dispose();
    }
}
