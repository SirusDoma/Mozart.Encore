using System.Net;
using Encore.Messaging;
using Encore.Metadata;
using Memoryer.Relay.Messages.Codecs;

namespace Memoryer.Relay.Messages.Events;

public class ComboStartedEventData : IMessage
{
    public static Enum Command => RelayCommand.ComboStarted;

    [MessageField(order: 0)]
    private int Code { get; init; }

    [CollectionMessageField<IPEndpointCodec>(order: 1, prefixSizeType: TypeCode.Empty, minCount: 3, maxCount: 3)]
    public IReadOnlyList<IPEndPoint> RelayEndpoints { get; init; } = [];

    [MessageField(order: 2)]
    private ushort Unused { get; init; }

    [MessageField<MessageFieldCodec<uint>>(order: 3)]
    public bool Autoplay { get; init; } // Suppose to be a flag for enable/disable autoplay, but it is hardcoded to 1 instead.

    [MessageField<MessageFieldCodec<ushort>>(order: 4)]
    public MemberRole Role { get; init; }
}
