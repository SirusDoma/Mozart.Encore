using System.Net;
using System.Net.Sockets;
using Encore.Messaging;

namespace Memoryer.Relay.Sessions;

public class UdpRelayPeer : IRelayPeer
{
    private readonly UdpClient _transport;
    private readonly IMessageCodec _codec;
    private object? _token;

    public UdpRelayPeer(UdpClient transport, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, IMessageCodec codec)
    {
        _transport     = transport;
        _codec         = codec;
        LocalEndPoint  = localEndPoint;
        RemoteEndPoint = remoteEndPoint;
    }

    public IPEndPoint LocalEndPoint { get; }

    public IPEndPoint RemoteEndPoint { get; internal set; }

    public byte RecvSequence { get; set; } = 0;
    public byte SendSequence { get; set; } = 0;

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

    public async Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        _codec.Register<TMessage>();
        await WriteFrame(_codec.Encode(message), cancellationToken).ConfigureAwait(false);
    }
}
