using System.Buffers.Binary;
using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;
using Encore.Server.Sessions;
using Microsoft.Extensions.Options;

namespace Encore.Workers.Gateway;

public partial class ClientSession(
    TcpClient client,
    IOptions<TcpOptions> options,
    IMessageFramerFactory framer,
    ICommandDispatcher dispatcher, IMessageCodec codec
) : Session(client, options, framer, dispatcher, codec)
{
    private ChannelSession? _channelSession;

    private static partial bool IsGatewayCommand(ushort command);

    public void Register(ChannelSession session)
    {
        base.Register(session.Channel!);
        Interlocked.Exchange(ref _channelSession, session)?.Terminate();
    }

    public void Exit(ChannelSession session)
    {
        if (Interlocked.CompareExchange(ref _channelSession, null, session) == session)
            Terminate();
    }

    public override void Terminate()
    {
        Interlocked.Exchange(ref _channelSession, null)?.Terminate();
        base.Terminate();
    }

    protected override async Task OnFrameReceived(byte[] payload, CancellationToken cancellationToken)
    {
        var session = _channelSession;
        if (session != null && payload.Length >= 2 && !IsGatewayCommand(BinaryPrimitives.ReadUInt16LittleEndian(payload)))
        {
            await session.WriteFrame(payload, cancellationToken).ConfigureAwait(false);
            return;
        }

        await base.OnFrameReceived(payload, cancellationToken).ConfigureAwait(false);
    }
}
