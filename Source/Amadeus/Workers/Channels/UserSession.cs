using System.Net.Sockets;
using Microsoft.Extensions.Options;

using Encore.Messaging;
using Encore.Server;

using Mozart.Sessions;

namespace Amadeus.Workers.Channels;

public sealed class UserSession : Session
{
    private readonly IGatewayClient _gatewayClient;
    private readonly string _sessionId;

    public override bool Connected { get; protected set; } = true;

    public UserSession(
        string sessionId,
        TcpClient client,
        IGatewayClient gatewayClient,
        IOptions<TcpOptions> options,
        IMessageFramerFactory framerFactory,
        ICommandDispatcher dispatcher,
        IMessageCodec codec) 
        : base(client, options, framerFactory, dispatcher, codec)
    {
        _sessionId = sessionId;
        _gatewayClient = gatewayClient;
    }

    public string SessionId => _sessionId;

    public override async Task WriteFrame(byte[] frame, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(SessionId))
            await _gatewayClient.WriteFrame(frame, cancellationToken);
        else
            await _gatewayClient.WriteFrame(_sessionId, frame, cancellationToken);
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            // Channel sessions don't actively read from a connection
            // They receive messages pushed from the channel client
            // So we just wait for cancellation
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            TriggerDisconnectEvent();
        }
    }

    public override void Terminate()
    {
        Connected = false;
    }

    public async Task Dispatch(byte[] frame, CancellationToken cancellationToken = default)
    {
        await OnFrameReceived(frame, cancellationToken).ConfigureAwait(false);
    }
}
