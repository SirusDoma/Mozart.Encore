using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;

using Encore;
using Encore.Messaging;
using Encore.Server;

namespace Amadeus.Workers.Gateway;

public class ChannelStats
{
    public int GatewayId { get; init; }
    public int ChannelId { get; init; }
    public int Capacity  { get; init; }
    public int UserCount { get; init; }
}

public class ChannelSession : Encore.Sessions.Session
{
    private readonly ConcurrentDictionary<string, ClientSession> _clients = [];

    public ChannelSession(
        TcpClient client,
        IOptions<TcpOptions> options,
        IMessageFramerFactory framer,
        ICommandDispatcher dispatcher)
        : base(client, options.Value, framer, dispatcher)
    {
    }

    public int GatewayId { get; private set; }
    public int ChannelId { get; private set; }

    public void Authorize(int gatewayId, int channelId)
    {
        GatewayId = gatewayId;
        ChannelId = channelId;

        Authorize(channelId);
    }

    public async Task<string> Register(ClientSession session, CancellationToken cancellationToken)
    {
        string sessionId    = session.GetAuthorizedToken().ToString()!;
        _clients[sessionId] = session;

        session.Assign(sessionId);
        await SendChannelCommand(sessionId, GatewayCommand.GrantSession, cancellationToken);
        return sessionId;
    }

    public async Task WaitForAuthorization(TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            var delay = Task.Delay(timeout, cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Authorized)
                    return;

                if (delay.IsCompleted)
                    throw new TimeoutException();
            }
        }
        catch (Exception)
        {
            TriggerDisconnectEvent();
            throw;
        }

        await Task.FromCanceled(cancellationToken);
    }

    public async Task RequestChannelStats(string requestId, CancellationToken cancellationToken)
    {
        await SendChannelCommand(requestId, GatewayCommand.GetChannelStats, cancellationToken);
    }

    public IReadOnlyList<ClientSession> GetClientSessions()
    {
        return _clients.Values.ToList();
    }

    public void Terminate(ClientSession session)
    {
        _clients.TryRemove(session.Id, out _);
        _ = SendChannelCommand(session.Id, GatewayCommand.RevokeSession, CancellationToken.None);
    }

    private async Task SendChannelCommand(GatewayCommand command, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await using var writer = new BinaryWriter(stream);

        writer.Write((ushort)command);
        writer.Flush();

        await WriteFrame(stream.ToArray(), cancellationToken);
    }

    private async Task SendChannelCommand(string sessionId, GatewayCommand command, CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream();
        await using var writer = new BinaryWriter(stream);

        writer.Write((ushort)command);
        writer.Write(sessionId, Encoding.UTF8);
        writer.Flush();

        await WriteFrame(stream.ToArray(), cancellationToken);
    }
} 