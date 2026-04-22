using System.Net;
using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;
using Microsoft.Extensions.Options;

namespace Memoryer.Relay.Sessions;

public class RelaySession : Encore.Sessions.Session
{
    private readonly IMessageCodec _codec;

    public IPEndPoint LocalEndPoint => Client.Client.LocalEndPoint as IPEndPoint ?? new IPEndPoint(IPAddress.None, 0);
    public IPEndPoint RemoteEndPoint => Client.Client.RemoteEndPoint as IPEndPoint ?? new IPEndPoint(IPAddress.None, 0);

    public RelaySession(TcpClient client, IOptions<RelayOptions> options, IMessageFramerFactory framer,
        ICommandDispatcher dispatcher, IMessageCodec codec)
        : base(client, new TcpOptions { PacketBufferSize = options.Value.PacketBufferSize }, framer, dispatcher)
    {
        _codec = codec;
    }

    public async Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        _codec.Register<TMessage>();
        await WriteFrame(_codec.Encode(message), cancellationToken).ConfigureAwait(false);
    }
}
