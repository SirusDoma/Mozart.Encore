using Memoryer.Relay.Sessions;

namespace Memoryer.Relay.Services;

public interface IRelaySessionLocator
{
    IRelayPeer? FindByKeys(int sessionKey1, int sessionKey2);

    IReadOnlyList<IRelayPeer> GetPeers();
}
