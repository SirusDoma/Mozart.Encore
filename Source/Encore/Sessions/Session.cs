using System.Collections.Concurrent;
using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;

namespace Encore.Sessions;

public class Session : IDisposable
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

    public virtual bool Connected { get; protected set; }

    public virtual bool Authorized => _token != null;

    public virtual void Authorize<T>(T token)
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
        try
        {
            return (T)GetAuthorizedToken();
        }
        catch (InvalidCastException)
        {
            throw new InvalidOperationException(
                $"Token type mismatch (Expected: {typeof(T).Name} / Actual: {_token?.GetType().Name})");
        }
    }

    public virtual async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            Connected = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[] frame = await ReadFrame(cancellationToken).ConfigureAwait(false);
                await OnFrameReceived(frame, cancellationToken).ConfigureAwait(false);

                // Check if the recent operation disconnect the socket
                if (!Client.Connected)
                    break;
            }
        }
        finally
        {
            TriggerDisconnectEvent();
        }
    }

    protected virtual async Task OnFrameReceived(byte[] payload, CancellationToken cancellationToken)
    {
        if (payload.Length > 0)
            await Dispatcher.Dispatch(this, payload, cancellationToken).ConfigureAwait(false);
    }

    public virtual void Terminate()
    {
        Client.Close();
    }

    protected virtual void OnDisconnected()
    {
    }

    public virtual async Task WriteFrame(byte[] frame, CancellationToken cancellationToken)
    {
        await Framer.WriteFrame(frame, cancellationToken);
    }

    public virtual async ValueTask<byte[]> ReadFrame(CancellationToken cancellationToken)
    {
        return await Framer.ReadFrame(Options.PacketBufferSize, cancellationToken);
    }

    protected void TriggerDisconnectEvent()
    {
        Connected = false;

        OnDisconnected();
        Disconnected?.Invoke(this, EventArgs.Empty);
        Disconnected = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Client.Dispose();
    }
}