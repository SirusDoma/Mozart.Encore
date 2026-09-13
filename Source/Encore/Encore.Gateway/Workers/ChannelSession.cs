using System.Net.Sockets;
using Encore.Entities;
using Encore.Messaging;
using Encore.Server;
using Encore.Server.Sessions;
using Microsoft.Extensions.Options;

namespace Encore.Workers.Gateway;

public class ChannelSession : Session
{
    public ChannelSession(
        TcpClient client,
        IOptions<TcpOptions> options,
        IMessageFramerFactory framer,
        ICommandDispatcher dispatcher,
        IMessageCodec codec,
        ClientSession clientSession,
        IChannel channel
    ) : base(client, options, framer, dispatcher, codec)
    {
        ClientSession = clientSession;
        Channel = channel;
    }

    public ClientSession ClientSession { get; }

    protected override Task OnFrameReceived(byte[] payload, CancellationToken cancellationToken)
        => ClientSession.WriteFrame(payload, cancellationToken);

    protected override void OnDisconnected() => ClientSession.Exit(this);
}
