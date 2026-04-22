using System.Net;
using System.Net.Sockets;
using Encore.Sessions;
using Microsoft.Extensions.Options;

namespace Encore.Server;

public interface IUdpServer<TSession> : IDisposable
    where TSession : IUdpSession
{
    UdpClient Client { get; }

    bool Active { get; }

    UdpOptions Options { get; }

    void Start();

    Task Stop();

    Task<TSession> AcceptSession(CancellationToken cancellationToken);
}

public interface IUdpServer : IUdpServer<UdpSession>
{
}

public class UdpServer : UdpServer<UdpSession>, IUdpServer
{
    public UdpServer(IUdpSessionFactory factory, IOptions<UdpOptions> options)
        : base(factory, options)
    {
    }
}

public class UdpServer<TSession> : IUdpServer<TSession>
    where TSession : class, IUdpSession
{
    private readonly UdpClient _client;
    private readonly IUdpSessionFactory<TSession> _factory;
    private readonly IPEndPoint _localEp;

    public UdpServer(IUdpSessionFactory<TSession> factory, IOptions<UdpOptions> options)
    {
        Options  = options.Value;
        _localEp = new IPEndPoint(IPAddress.Parse(Options.Address), Options.Port);
        _client  = new UdpClient(_localEp.AddressFamily);
        _factory = factory;
    }

    public UdpClient Client => _client;

    public bool Active { get; private set; }

    public UdpOptions Options { get; }

    public void Start()
    {
        if (Active)
            return;

        if (Options.ReceiveBufferSize > 0)
            _client.Client.ReceiveBufferSize = Options.ReceiveBufferSize;

        _client.Client.Bind(_localEp);
        Active = true;
    }

    public Task Stop()
    {
        if (!Active)
            return Task.CompletedTask;

        Active = false;
        _client.Close();

        return Task.CompletedTask;
    }

    public async Task<TSession> AcceptSession(CancellationToken cancellationToken)
    {
        while (Active && !cancellationToken.IsCancellationRequested)
        {
            var result = await _client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
            if (!Active)
                break;

            if (result.Buffer.Length == 0)
                continue;

            return _factory.CreateSession(_client, result);
        }

        return await Task.FromCanceled<TSession>(cancellationToken);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Stop();
        _client.Dispose();
    }
}
