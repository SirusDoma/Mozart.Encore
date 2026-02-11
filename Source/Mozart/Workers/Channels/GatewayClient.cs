using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Options;

using Encore;
using Encore.Messaging;
using Encore.Server;

using Microsoft.Extensions.Logging;
using Mozart.Data.Repositories;
using Mozart.Internal.Requests;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Workers.Channels;

public interface IGatewayClient : IDisposable
{
    Socket Socket { get; }

    bool Connected { get; }

    Task Connect(CancellationToken cancellationToken = default);

    Task WriteFrame(byte[] payload, CancellationToken cancellationToken = default);

    Task WriteFrame(string sessionId, byte[] payload, CancellationToken cancellationToken = default);

    Task SendGatewayCommand(ChannelCommand command, CancellationToken cancellationToken = default);

    Task<UserSession> AcceptSession(CancellationToken cancellationToken = default);

    Task<UserSession> EnqueueSession(string sessionId, CancellationToken cancellationToken = default);

    Task RevokeSession(string sessionId, CancellationToken cancellationToken = default);

    Task Dispatch(UserSession session, byte[] frame, CancellationToken cancellationToken = default);
}

public class GatewayClient : IGatewayClient
{
    private readonly IUserSessionFactory _factory;
    private readonly ISessionManager _manager;
    private readonly ILogger<GatewayClient> _logger;
    private readonly GatewayOptions _gatewayOptions;
    private readonly TcpOptions _tcpOptions;
    private readonly IMessageFramerFactory _framerFactory;
    private readonly ICommandDispatcher _dispatcher;
    private readonly IMessageCodec _codec;

    private readonly TcpClient _client;

    private IMessageFramer? _framer;
    private bool _disposed;

    private readonly ConcurrentQueue<UserSession> _queue = [];
    private readonly ConcurrentDictionary<string, UserSession> _sessions = [];

    public GatewayClient(
        IUserSessionFactory factory,
        ISessionManager manager,
        ILogger<GatewayClient> logger,
        IOptions<TcpOptions> tcpOptions,
        IOptions<GatewayOptions> gatewayOptions,
        IMessageFramerFactory framerFactory,
        ICommandDispatcher dispatcher,
        IMessageCodec codec)
    {
        _factory         = factory;
        _manager         = manager;
        _logger          = logger;

        _tcpOptions     = tcpOptions.Value;
        _gatewayOptions = gatewayOptions.Value;
        _codec          = codec;
        _framerFactory  = framerFactory;
        _dispatcher     = dispatcher;

        _client = new TcpClient(new IPEndPoint(
            address: IPAddress.Parse(_tcpOptions.Address),
            port:    _tcpOptions.Port
        ));
    }

    public Socket Socket => _client.Client;

    public bool Connected => _client.Connected;

    public async Task Connect(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {

            await _client.ConnectAsync(
                IPAddress.Parse(_gatewayOptions.Address),
                _gatewayOptions.Port
            ).WaitAsync(
                TimeSpan.FromSeconds(_gatewayOptions.Timeout),
                cancellationToken
            );

            var options = _gatewayOptions.Channels.First();
            _framer     = _framerFactory.CreateFramer(_client.GetStream());

            // Start-up message: for channel discovery
            await _framer.WriteFrame(_codec.Encode(new CreateChannelRequest()
            {
                GatewayId = _gatewayOptions.Id,
                ChannelId = options.Id
            }), cancellationToken);

            // Start-up response: ensure gateway id match
            var task  = ReadCreateChannelResponse(cancellationToken);
            var delay = Task.Delay(TimeSpan.FromSeconds(_gatewayOptions.Timeout), cancellationToken);

            await Task.WhenAny(task, delay);

            if (!task.IsCompletedSuccessfully)
            {
                if (task.IsFaulted)
                    throw task.Exception;

                throw new TimeoutException();
            }

            var response = task.Result;
            if (!response.Success)
            {
                throw new InvalidOperationException(
                    "Failed to register channel (Check gateway and channel configuration)");
            }
        }
        catch (Exception)
        {
            Dispose();
            throw;
        }
    }

    public async Task Disconnect(CancellationToken cancellationToken = default)
    {
        Dispose();
        await Task.CompletedTask;
    }

    public async Task ReadFrame(CancellationToken cancellationToken = default)
    {
        if (_framer == null)
            throw new InvalidOperationException("Not connected to gateway server");

        byte[] frame = await _framer.ReadFrame(_tcpOptions.PacketBufferSize, cancellationToken);

        using var stream = new MemoryStream(frame);
        using var reader = new BinaryReader(stream);

        var command = (GatewayCommand)reader.ReadUInt16();
        string sessionId = string.Empty;
        if (stream.Position < stream.Length - 1)
            sessionId = reader.ReadString(Encoding.UTF8, TypeCode.Empty, true, 128);

        if (!_sessions.TryGetValue(sessionId, out var session))
            session = _factory.CreateSession(_client, string.Empty);

        await _dispatcher.Dispatch(session, frame, cancellationToken);
    }

    private async Task<CreateChannelResponse> ReadCreateChannelResponse(CancellationToken cancellationToken = default)
    {
        if (_framer == null)
            throw new InvalidOperationException("Not connected to gateway server");

        byte[] buffer = await _framer.ReadFrame(_tcpOptions.PacketBufferSize, cancellationToken);
        return _codec.Decode<CreateChannelResponse>(buffer);
    }

    public async Task Dispatch(UserSession session, byte[] frame, CancellationToken cancellationToken = default)
    {
        await session.Dispatch(frame, cancellationToken);
        if (!session.Connected)
            _sessions.TryRemove(session.SessionId, out _);
    }

    public async Task<UserSession> AcceptSession(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var session))
                return session;

            await ReadFrame(cancellationToken);
        }

        return await Task.FromCanceled<UserSession>(cancellationToken);
    }

    public Task<UserSession> EnqueueSession(string sessionId, CancellationToken cancellationToken = default)
    {
        var session = _factory.CreateSession(_client, sessionId);
        _queue.Enqueue(session);
        _sessions[sessionId] = session;

        return Task.FromResult(session);
    }

    public async Task RevokeSession(string sessionId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            _logger.LogInformation("Session stop requested");
            await _manager.StopSession(session);
        }
    }

    public async Task SendGatewayCommand(ChannelCommand command, CancellationToken cancellationToken = default)
    {
        if (_framer == null)
            throw new InvalidOperationException("Not connected to gateway server");

        byte[] frame;
        await using (var stream = new MemoryStream())
        await using (var writer = new BinaryWriter(stream))
        {
            writer.Write((ushort)command);
            writer.Flush();

            frame = stream.ToArray();
        }

        await _framer.WriteFrame(frame, cancellationToken);
    }

    public async Task WriteFrame(string sessionId, byte[] payload, CancellationToken cancellationToken = default)
    {
        if (_framer == null)
            throw new InvalidOperationException("Not connected to gateway server");

        byte[] frame;
        await using (var stream = new MemoryStream())
        await using (var writer = new BinaryWriter(stream))
        {
            writer.Write((ushort)ChannelCommand.Relay);
            writer.Write(sessionId, Encoding.UTF8, TypeCode.Empty, true, 128);
            writer.Write(payload);
            writer.Flush();

            frame = stream.ToArray();
        }

        await _framer.WriteFrame(frame, cancellationToken);
    }


    public async Task WriteFrame(byte[] payload, CancellationToken cancellationToken = default)
    {
        if (_framer == null)
            throw new InvalidOperationException("Not connected to gateway server");

        byte[] frame;
        await using (var stream = new MemoryStream())
        await using (var writer = new BinaryWriter(stream))
        {
            writer.Write(payload);
            writer.Flush();

            frame = stream.ToArray();
        }

        await _framer.WriteFrame(frame, cancellationToken);
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_disposed)
            return;

        _client.Close();
        _client.Dispose();
        _disposed = true;
    }
}
 