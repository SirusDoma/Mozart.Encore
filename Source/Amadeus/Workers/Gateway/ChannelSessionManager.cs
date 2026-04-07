using System.Collections.Concurrent;
using Encore.Sessions;

namespace Amadeus.Workers.Gateway;

public interface IChannelSessionManager : ISessionManager<ChannelSession>
{
    void Authorize(ChannelSession session);

    ChannelSession? GetChannelSession(int channelId);

    IReadOnlyList<ChannelSession> GetChannelSessions();
}

public class ChannelSessionManager : SessionManager<ChannelSession>, IChannelSessionManager
{
    private readonly ConcurrentDictionary<int, ChannelSession> _sessions = [];

    public ChannelSession? GetChannelSession(int channelId)
    {
        return _sessions.GetValueOrDefault(channelId);
    }

    public IReadOnlyList<ChannelSession> GetChannelSessions()
    {
        return _sessions.Values.ToList();
    }

    public override void StartSession(ChannelSession session)
    {
        if (Validate(session))
            return;

        base.StartSession(session);
    }

    public void Authorize(ChannelSession session)
    {
        if (!_sessions.TryAdd(session.ChannelId, session))
            throw new InvalidOperationException("Channel session is already exists");
    }

    public override Task StopSession(ChannelSession session)
    {
        var execution = base.StopSession(session);
        _sessions.TryRemove(session.ChannelId, out _);

        return execution;
    }

}
