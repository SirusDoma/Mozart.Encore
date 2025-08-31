using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class ChannelListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetChannelList;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<ChannelState> Channels { get; init; }

    public class ChannelState : SubMessage
    {
        [MessageField(order: 0)]
        public ushort ServerId { get; init; }

        [MessageField(order: 1)]
        public ushort ChannelId { get; init; }

        [MessageField(order: 2)]
        public int Capacity { get; init; }

        [MessageField(order: 3)]
        public int Population { get; init; }

        [MessageField(order: 4)]
        public bool Active { get; init; }

    }
}