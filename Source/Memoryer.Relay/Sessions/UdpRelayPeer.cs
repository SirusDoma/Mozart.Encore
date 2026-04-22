using System.Net;
using System.Net.Sockets;
using Encore.Messaging;
using Memoryer.Relay.Messaging;
using Microsoft.Extensions.Options;

namespace Memoryer.Relay.Sessions;

public class UdpRelayPeer : IRelayPeer, IAsyncDisposable
{
    private readonly UdpClient              _transport;
    private readonly UdpReliabilityChannel  _channel;
    private          object?                _token;

    public UdpRelayPeer(
        UdpClient                  transport,
        IPEndPoint                 localEndPoint,
        IPEndPoint                 remoteEndPoint,
        IUdpRelayFrameCodec        codec,
        IOptions<RelayOptions>     options
    )
    {
        _transport     = transport;
        _channel       = new UdpReliabilityChannel(
            this,
            codec,
            options.Value.RetransmissionInterval,
            options.Value.MaxRetransmissionAttempts);
        LocalEndPoint  = localEndPoint;
        RemoteEndPoint = remoteEndPoint;
    }

    public IPEndPoint LocalEndPoint  { get; }
    public IPEndPoint RemoteEndPoint { get; internal set; }

    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

    public bool Authorized => _token != null;

    public void Authorize<T>(T token)
    {
        _token = token;
    }

    public T GetAuthorizedToken<T>()
    {
        if (_token == null)
            throw new InvalidOperationException("Unauthorized");

        return (T)_token;
    }

    public Task Execute(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task WriteFrame(byte[] payload, CancellationToken cancellationToken)
    {
        await _transport.SendAsync(payload, RemoteEndPoint, cancellationToken).ConfigureAwait(false);
    }

    public Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
        => _channel.SendReliableAsync(message, cancellationToken);

    public Task WriteUnreliableMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
        => _channel.SendUnreliableAsync(message, cancellationToken);

    public Task<byte[]?> ReceiveAsync(byte[] datagram, CancellationToken cancellationToken)
        => _channel.ReceiveAsync(datagram, cancellationToken);

    public ValueTask DisposeAsync() => _channel.DisposeAsync();
}
