using System.Net;
using Encore.Messaging;
using Memoryer.Relay.Messages.Codecs;

namespace Memoryer.Relay.Messages;

public class ConnectionEndpoint : SubMessage
{
    [MessageField(order: 0)]
    public ushort Port1 { get; init; }

    [MessageField(order: 1)]
    public ushort Port2 { get; init; }

    [MessageField<IPAddressCodec>(order: 2)]
    public IPAddress Address { get; init; } = IPAddress.None;

    public static implicit operator IPEndPoint(ConnectionEndpoint ep)
    {
        return new IPEndPoint(ep.Address, ep.Port1);
    }

    public static implicit operator ConnectionEndpoint(IPEndPoint ep)
    {
        return new ConnectionEndpoint
        {
            Address = ep.Address,
            Port1   = (ushort)ep.Port
        };
    }

    public override string ToString()
    {
        return $"{Address}:{Port1}:{Port2}";
    }
}
