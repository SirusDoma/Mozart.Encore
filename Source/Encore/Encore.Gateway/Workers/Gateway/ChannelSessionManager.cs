using Encore.Server.Sessions;
using Encore.Sessions;

namespace Encore.Workers.Gateway;

public interface IChannelSessionManager : ITcpSessionManager<Session>;

public class ChannelSessionManager : TcpSessionManager<Session>, IChannelSessionManager
{
    private readonly Lock _lock = new();

    public override bool Validate(Session session)
    {
        lock (_lock)
            return base.Validate(session);
    }

    public override IReadOnlyList<Session> GetSessions()
    {
        lock (_lock)
            return base.GetSessions();
    }

    public override void StartSession(Session session)
    {
        lock (_lock)
            base.StartSession(session);
    }

    public override Task StopSession(Session session)
    {
        lock (_lock)
            return base.StopSession(session);
    }

    public new Task ClearSessions()
    {
        lock (_lock)
            return base.ClearSessions();
    }

    public override void Dispose() => ClearSessions();
}
