using System.Net;
using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;
using Encore.Sessions;
using Microsoft.Extensions.Options;

namespace Memoryer.Relay.Sessions;

public class UdpRelaySession : UdpSession, IRelayPeer
{
    private readonly UdpRelayPeer _peer;

    public UdpRelaySession(
        UdpClient                transport,
        IOptions<RelayOptions>   options,
        ICommandDispatcher       dispatcher,
        UdpReceiveResult         result,
        IUdpRelayPeerRegistry    registry
    )
        : base(transport, new UdpOptions
        {
            Address           = options.Value.Endpoints.FirstOrDefault()?.Address ?? "0.0.0.0",
            Port              = 0,
            ReceiveBufferSize = options.Value.PacketBufferSize
        }, dispatcher, result)
    {
        _peer = registry.GetOrCreate(transport, result.RemoteEndPoint);
    }

    public IPEndPoint LocalEndPoint => _peer.LocalEndPoint;

    public bool Authorized => _peer.Authorized;

    public void Authorize<T>(T token) => _peer.Authorize(token);

    public T GetAuthorizedToken<T>() => _peer.GetAuthorizedToken<T>();

    protected override async Task OnFrameReceived(byte[] datagram, CancellationToken cancellationToken)
    {
        byte[]? innerPayload = await _peer.ReceiveAsync(datagram, cancellationToken).ConfigureAwait(false);
        if (innerPayload != null)
            await base.OnFrameReceived(innerPayload, cancellationToken).ConfigureAwait(false);
    }

    public Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
        => _peer.WriteMessage(message, cancellationToken);
}
