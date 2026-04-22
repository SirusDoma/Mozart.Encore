using System.Collections.Concurrent;
using Encore.Sessions;

namespace Memoryer.Relay.Sessions;

public interface IRelaySessionManager : ISessionManager<RelaySession>
{
    IReadOnlyList<RelaySession> GetSessions();

    RelaySession? FindByKeys(int sessionKey1, int sessionKey2);
}

public class RelaySessionManager : SessionManager<RelaySession>, IRelaySessionManager
{
    private readonly ConcurrentDictionary<RelaySession, byte> _tracked = new();

    public RelaySessionManager()
    {
        Started += (_, e) =>
        {
            if (e.Session is RelaySession session)
                _tracked.TryAdd(session, 0);
        };

        Stopped += (_, e) =>
        {
            if (e.Session is RelaySession session)
                _tracked.TryRemove(session, out byte _);
        };
    }

    public IReadOnlyList<RelaySession> GetSessions()
    {
        return _tracked.Keys.ToList();
    }

    public RelaySession? FindByKeys(int sessionKey1, int sessionKey2)
    {
        foreach (var session in _tracked.Keys)
        {
            if (!session.Authorized)
                continue;

            try
            {
                var actor = session.GetAuthorizedToken<RelayActor>();
                if (actor.SessionKey1 == sessionKey1 && actor.SessionKey2 == sessionKey2)
                    return session;
            }
            catch (InvalidOperationException)
            {
                // Session is authorized with a non-RelayActor token; skip.
            }
        }

        return null;
    }
}
