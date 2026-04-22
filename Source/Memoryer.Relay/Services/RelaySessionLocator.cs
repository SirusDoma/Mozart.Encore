using Memoryer.Relay.Sessions;

namespace Memoryer.Relay.Services;

public class RelaySessionLocator(
    ITcpRelaySessionManager tcp,
    IUdpRelayPeerRegistry udp
) : IRelaySessionLocator
{
    public IRelayPeer? FindByKeys(int sessionKey1, int sessionKey2)
    {
        return (IRelayPeer?)tcp.FindByKeys(sessionKey1, sessionKey2)
               ?? udp.FindByKeys(sessionKey1, sessionKey2);
    }

    public IReadOnlyList<IRelayPeer> GetPeers()
    {
        return tcp.GetSessions()
            .Cast<IRelayPeer>()
            .Concat(udp.GetPeers().Cast<IRelayPeer>())
            .ToList();
    }
}
