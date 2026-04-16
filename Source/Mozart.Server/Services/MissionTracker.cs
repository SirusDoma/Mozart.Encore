using System.Collections.Concurrent;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IMissionTracker
{
    void Track(Session session, int missionLevel, int musicId, int serverId);
    MissionTracker.MissionState Complete(Session session);
}

public class MissionTracker : IMissionTracker
{
    public class MissionState
    {
        public required Session Session  { get; init; }
        public required int MissionLevel { get; init; }
        public required int MusicId      { get; init; }
        public required int ServerId     { get; init; }
    }

    private readonly ConcurrentDictionary<Session, MissionState> _states = [];

    public void Track(Session session, int missionLevel, int musicId, int serverId)
    {
        if (_states.ContainsKey(session))
            throw new InvalidOperationException("Session is already tracked");

        var state = new MissionState
        {
            Session      = session,
            MissionLevel = missionLevel,
            MusicId      = musicId,
            ServerId     = serverId
        };

        _states[session] = state;
        session.Disconnected += OnSessionDisconnected;
    }

    public MissionState Complete(Session session)
    {
        if (!_states.TryRemove(session, out var state))
            throw new InvalidOperationException("Session is not tracked");

        session.Disconnected -= OnSessionDisconnected;
        return state;
    }

    private void OnSessionDisconnected(object? sender, EventArgs e)
    {
        var session = (Session)sender!;
        if (_states.TryRemove(session, out _))
            session.Disconnected -= OnSessionDisconnected;
    }
}
