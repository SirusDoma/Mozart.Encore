using Encore.Messaging;

namespace Encore.Messages.Responses;

public class ChannelRemovedResponse : IMessage
{
    public static Enum Command => GameManagerCommand.ChannelRemoved;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<ChannelEntry> Channels { get; init; } = [];

    public class ChannelEntry : SubMessage
    {
        [MessageField(order: 0)]
        public ushort GatewayId { get; init; }

        [MessageField(order: 1)]
        public ushort ChannelId { get; init; }
    }
}
