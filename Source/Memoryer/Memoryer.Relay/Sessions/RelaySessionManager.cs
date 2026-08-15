using System.Collections.Concurrent;
using Encore.Sessions;

namespace Memoryer.Relay.Sessions;

public interface ITcpRelaySessionManager : ITcpSessionManager<TcpRelaySession>
{
    IReadOnlyList<TcpRelaySession> GetSessions();

    TcpRelaySession? FindByKeys(int sessionKey1, int sessionKey2);
}

public class TcpRelaySessionManager : TcpSessionManager<TcpRelaySession>, ITcpRelaySessionManager
{
    private readonly ConcurrentDictionary<TcpRelaySession, byte> _tracked = new();
    private readonly IUdpRelayPeerRegistry? _udpPeers;

    public TcpRelaySessionManager(IUdpRelayPeerRegistry? udpPeers = null)
    {
        _udpPeers = udpPeers;

        Started += (_, e) =>
        {
            if (e.Session is { } session)
                _tracked.TryAdd(session, 0);
        };

        Stopped += (_, e) =>
        {
            if (e.Session is not { } session)
                return;

            _tracked.TryRemove(session, out byte _);
            UnregisterUdpPeer(session);
        };
    }

    private void UnregisterUdpPeer(TcpRelaySession session)
    {
        if (_udpPeers == null || !session.Authorized)
            return;

        RelayActor actor;
        try
        {
            actor = session.GetAuthorizedToken<RelayActor>();
        }
        catch (InvalidOperationException)
        {
            return;
        }

        var peer = _udpPeers.FindByKeys(actor.SessionKey1, actor.SessionKey2);
        if (peer != null)
            _udpPeers.Remove(peer);
    }

    public IReadOnlyList<TcpRelaySession> GetSessions()
    {
        return _tracked.Keys.ToList();
    }

    public TcpRelaySession? FindByKeys(int sessionKey1, int sessionKey2)
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
