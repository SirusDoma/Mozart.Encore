using System.Net;
using System.Net.Sockets;
using Encore.Server;

namespace Encore.Sessions;

public class UdpSession : IUdpSession
{
    private readonly UdpClient _transport;
    private readonly byte[] _payload;

    public UdpSession(
        UdpClient transport,
        UdpOptions options,
        ICommandDispatcher dispatcher,
        UdpReceiveResult result
    )
    {
        _transport     = transport;
        _payload       = result.Buffer;
        Options        = options;
        Dispatcher     = dispatcher;
        RemoteEndPoint = result.RemoteEndPoint;
    }

    public UdpOptions Options { get; }

    public IPEndPoint RemoteEndPoint { get; }

    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

    protected ICommandDispatcher Dispatcher { get; }

    public virtual Task Execute(CancellationToken cancellationToken)
    {
        return OnFrameReceived(_payload, cancellationToken);
    }

    protected virtual async Task OnFrameReceived(byte[] payload, CancellationToken cancellationToken)
    {
        if (payload.Length > 0)
            await Dispatcher.Dispatch(this, payload, cancellationToken).ConfigureAwait(false);
    }

    public virtual async Task WriteFrame(byte[] payload, CancellationToken cancellationToken)
    {
        await _transport.SendAsync(payload, RemoteEndPoint, cancellationToken).ConfigureAwait(false);
    }
}
