using System.Net;
using Encore.Messaging;
using Memoryer.Relay.Messages.Codecs;
using Mozart.Metadata;

namespace Memoryer.Relay.Messages.Events;

public class ComboBrokenEventData : IMessage
{
    public static Enum Command => RelayCommand.ComboBroken;

    [MessageField(order: 0)]
    private int Code { get; init; }

    [CollectionMessageField<IPEndpointCodec>(order: 1, prefixSizeType: TypeCode.Empty, minCount: 3, maxCount: 3)]
    public IReadOnlyList<IPEndPoint> RelayEndpoints { get; init; } = [];

    [MessageField(order: 2)]
    private ushort Unused { get; init; }

    [MessageField(order: 4)]
    public uint UnusedFlag { get; init; } // Suppose to be flag enable/disable autoplay, but it is constant set to 1 instead.

    [MessageField<MessageFieldCodec<ushort>>(order: 3)]
    public RoomLiveRole Role { get; init; }
}
