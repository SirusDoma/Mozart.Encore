using System.Collections.Concurrent;
using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;

namespace Encore.Sessions;

public class Session
{
    private object? _token = null;

    public Session(
        TcpClient client,
        TcpOptions options,
        IMessageFramerFactory framer,
        ICommandDispatcher dispatcher
    )
    {
        Client     = client;
        Options    = options;
        Framer     = framer.CreateFramer(client.GetStream());
        Dispatcher = dispatcher;
    }

    public event EventHandler? Disconnected;

    public Socket Socket => Client.Client;

    protected TcpClient Client { get; }

    protected IMessageFramer Framer { get; }

    protected ICommandDispatcher Dispatcher { get; }

    public TcpOptions Options { get; private set; }

    public ConcurrentDictionary<string, object> Properties { get; } = [];

    public virtual bool Authorized => _token != null;

    public virtual void Authorize<T>(T token)
        where T : class
    {
        _token = token;
    }

    public object GetAuthorizedToken()
    {
        if (!Authorized)
            throw new InvalidOperationException("Unauthorized");

        return _token!;
    }

    public virtual T GetAuthorizedToken<T>()
    {
        return (T)GetAuthorizedToken();
    }

    public virtual async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            byte[] frame = await ReadFrameAsync(cancellationToken).ConfigureAwait(false);
            if (frame.Length > 0)
                await OnFrameReceived(frame, cancellationToken);

            if (!Client.Connected)
                break;
        }

        if (!Client.Connected)
        {
            OnDisconnected();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual async Task OnFrameReceived(byte[] payload, CancellationToken cancellationToken)
    {
        await Dispatcher.DispatchAsync(this, payload, cancellationToken);
    }

    public void Terminate()
    {
        Client.Close();
    }

    protected virtual void OnDisconnected()
    {
    }

    public async Task WriteFrameAsync(byte[] frame, CancellationToken cancellationToken)
    {
        await Framer.WriteFrameAsync(frame, cancellationToken);
    }

    public async ValueTask<byte[]> ReadFrameAsync(CancellationToken cancellationToken)
    {
        try
        {
            byte[] buffer = new byte[Options.PacketBufferSize];
            int count = await Framer.ReadFrameAsync(buffer, cancellationToken);

            return count <= 0 ? [] : buffer[..count];
        }
        catch (EndOfStreamException)
        {
            return [];
        }
    }
}