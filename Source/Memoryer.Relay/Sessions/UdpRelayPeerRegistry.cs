using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Memoryer.Relay.Messaging;
using Microsoft.Extensions.Options;

namespace Memoryer.Relay.Sessions;

public class UdpRelayPeerRegistry : IUdpRelayPeerRegistry
{
    private readonly ConcurrentDictionary<IPEndPoint, UdpRelayPeer> _peers = new();
    private readonly IUdpRelayFrameCodec    _codec;
    private readonly IOptions<RelayOptions> _options;

    public UdpRelayPeerRegistry(IUdpRelayFrameCodec codec, IOptions<RelayOptions> options)
    {
        _codec   = codec;
        _options = options;
    }

    public UdpRelayPeer GetOrCreate(UdpClient transport, IPEndPoint remoteEndPoint)
    {
        return _peers.AddOrUpdate(
            remoteEndPoint,
            ep =>
            {
                var local = (IPEndPoint?)transport.Client.LocalEndPoint
                            ?? new IPEndPoint(IPAddress.None, 0);
                return new UdpRelayPeer(transport, local, ep, _codec, _options);
            },
            (_, existing) =>
            {
                existing.RemoteEndPoint = remoteEndPoint;
                return existing;
            }
        );
    }

    public UdpRelayPeer? FindByKeys(int sessionKey1, int sessionKey2)
    {
        foreach (var peer in _peers.Values)
        {
            if (!peer.Authorized)
                continue;

            try
            {
                var actor = peer.GetAuthorizedToken<RelayActor>();
                if (actor.SessionKey1 == sessionKey1 && actor.SessionKey2 == sessionKey2)
                    return peer;
            }
            catch (InvalidOperationException)
            {
                // Authorized with a non-RelayActor token; skip.
            }
        }

        return null;
    }

    public IReadOnlyList<UdpRelayPeer> GetPeers()
    {
        return _peers.Values.ToList();
    }

    public bool Remove(UdpRelayPeer peer)
    {
        if (!_peers.TryRemove(new KeyValuePair<IPEndPoint, UdpRelayPeer>(peer.RemoteEndPoint, peer)))
            return false;

        // Channel ownership transfers to the disposal task; surface failures via TaskScheduler.UnobservedTaskException.
        _ = peer.DisposeAsync().AsTask();
        return true;
    }
}
