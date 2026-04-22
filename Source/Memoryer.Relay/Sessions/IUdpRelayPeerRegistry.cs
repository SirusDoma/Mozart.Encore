using System.Net;
using System.Net.Sockets;

namespace Memoryer.Relay.Sessions;

public interface IUdpRelayPeerRegistry
{
    UdpRelayPeer GetOrCreate(UdpClient transport, IPEndPoint remoteEndPoint);

    UdpRelayPeer? FindByKeys(int sessionKey1, int sessionKey2);

    IReadOnlyList<UdpRelayPeer> GetPeers();
}
