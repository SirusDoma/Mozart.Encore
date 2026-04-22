using System.Collections.Concurrent;
using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;
using Encore.Sessions;
using Memoryer.Messages.Requests;
using Memoryer.Relay;
using Memoryer.Relay.Messages.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Memoryer.Services;

public class RelayService : IRelayService, IDisposable
{
    private readonly IOptions<RelayOptions> _options;
    private readonly IMessageFramerFactory _framer;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IMessageCodec _codec;
    private readonly ILogger<RelayService> _logger;

    private readonly ConcurrentDictionary<string, RelaySession> _connections = new();

    public RelayService(IOptions<RelayOptions> options, IMessageFramerFactory framer,
        ICommandDispatcher dispatcher, IMessageCodec codec, ILogger<RelayService> logger)
    {
        _options    = options;
        _framer     = framer;
        _dispatcher = dispatcher;
        _codec      = codec;
        _logger     = logger;
    }

    public Task CreateSession(CreateRelaySessionRequest request, CancellationToken cancellationToken)
        => Broadcast(request, cancellationToken);

    public Task DeleteSession(DeleteRelaySessionRequest request, CancellationToken cancellationToken)
        => Broadcast(request, cancellationToken);

    private async Task Broadcast<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        foreach (var endpoint in _options.Value.Endpoints)
        {
            try
            {
                var connection = await GetOrConnect(endpoint, cancellationToken);
                await connection.WriteMessage(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send {Message} to relay {Address}:{Port}",
                    typeof(TMessage).Name, endpoint.Address, endpoint.Port);
            }
        }
    }

    private async Task<RelaySession> GetOrConnect(RelayEndpoint endpoint, CancellationToken cancellationToken)
    {
        string key = $"{endpoint.Address}:{endpoint.Port}";

        while (_connections.TryGetValue(key, out var existing))
        {
            if (existing.IsAlive)
                return existing;

            if (_connections.TryRemove(new KeyValuePair<string, RelaySession>(key, existing)))
                existing.Dispose();
        }

        var session = await CreateConnection(endpoint, cancellationToken);
        if (_connections.TryAdd(key, session))
            return session;

        throw new InvalidOperationException();
    }

    private async Task<RelaySession> CreateConnection(RelayEndpoint endpoint, CancellationToken cancellationToken)
    {
        var tcp = new TcpClient();
        try
        {
            await tcp.ConnectAsync(endpoint.Address, endpoint.Port, cancellationToken);
        }
        catch
        {
            tcp.Dispose();
            throw;
        }

        return new RelaySession(tcp, _options.Value, _framer, _dispatcher, _codec);
    }

    public void Dispose()
    {
        foreach (var connection in _connections.Values)
            connection.Dispose();
        _connections.Clear();
    }

    private sealed class RelaySession : Session
    {
        private readonly IMessageCodec _codec;

        public RelaySession(TcpClient client, RelayOptions options, IMessageFramerFactory framer,
            ICommandDispatcher dispatcher, IMessageCodec codec)
            : base(client, new TcpOptions { PacketBufferSize = options.PacketBufferSize }, framer, dispatcher)
        {
            _codec = codec;
        }

        public bool IsAlive => Client.Connected;

        public async Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
            where TMessage : class, IMessage
        {
            _codec.Register<TMessage>();
            await WriteFrame(_codec.Encode(message), cancellationToken).ConfigureAwait(false);
        }
    }
}
