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

    public virtual async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] frame = await ReadFrame(cancellationToken).ConfigureAwait(false);
                if (frame.Length > 0)
                    await OnFrameReceived(frame, cancellationToken).ConfigureAwait(false);

                // Check if the recent operation disconnect the socket
                if (!Client.Connected)
                    break;
            }
        }
        finally
        {
            OnDisconnected();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual async Task OnFrameReceived(byte[] payload, CancellationToken cancellationToken)
    {
        await Dispatcher.Dispatch(this, payload, cancellationToken).ConfigureAwait(false);
    }

    public void Terminate()
    {
        Client.Close();
    }

    protected virtual void OnDisconnected()
    {
    }

    public async Task WriteFrame(byte[] frame, CancellationToken cancellationToken)
    {
        await Framer.WriteFrame(frame, cancellationToken);
    }

    public async ValueTask<byte[]> ReadFrame(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[Options.PacketBufferSize];
        int count = await Framer.ReadFrame(buffer, cancellationToken);

        return count <= 0 ? [] : buffer[..count];
    }
}