using System.Net.Sockets;
using Encore.Server;

namespace Encore.Sessions;

public interface ITcpSession : ISession, IDisposable
{
    event EventHandler? Disconnected;

    Socket Socket { get; }

    TcpOptions Options { get; }

    bool Connected { get; }

    bool Authorized { get; }

    void Authorize<T>(T token);

    object GetAuthorizedToken();

    T GetAuthorizedToken<T>();

    void Terminate();

    ValueTask<byte[]> ReadFrame(CancellationToken cancellationToken);
}
