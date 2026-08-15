using Encore.Sessions;

namespace Encore.Entities;

public interface IRoomLifecycle
{
    event EventHandler<SessionEventArgs<TcpSession>>? SessionDisconnected;
}
