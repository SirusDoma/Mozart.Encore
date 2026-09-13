using Encore.Messaging;

namespace Encore.Messages.Responses;

public class ChannelCreatedResponse : IMessage
{
    public static Enum Command => GameManagerCommand.ChannelCreated;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<ChannelEntry> Channels { get; init; } = [];

    public class ChannelEntry : SubMessage
    {
        [MessageField(order: 0)]
        public ushort ServerId { get; init; }

        [MessageField(order: 1)]
        public ushort ChannelId { get; init; }

        [StringMessageField(order: 2)]
        public string ServerAddress { get; init; } = string.Empty;

        [MessageField(order: 3)]
        public int Port { get; init; }

        [MessageField(order: 4)]
        public int Capacity { get; init; }

        [MessageField(order: 5)]
        public bool Active { get; init; }
    }
}
