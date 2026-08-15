using System.Net;
using Encore.Messaging;
using Memoryer.Relay.Messages.Codecs;

namespace Memoryer.Relay.Messages.Events;

public class PeerConnectedEventData : IMessage
{
    public static Enum Command => RelayCommand.PeerConnected;

    [MessageField(order: 0)]
    private int Code { get; init; } = 0;

    [CollectionMessageField<IPEndpointCodec>(order: 1, prefixSizeType: TypeCode.Empty, minCount: 3, maxCount: 3)]
    public IReadOnlyList<IPEndPoint> RelayEndpoints { get; init; } = [];

    [MessageField(order: 2)]
    private ushort Unused { get; init; } = 0;

    [MessageField(order: 3)]
    public int SessionKey1 { get; init; }

    [MessageField(order: 4)]
    public int SessionKey2 { get; init; }
}
