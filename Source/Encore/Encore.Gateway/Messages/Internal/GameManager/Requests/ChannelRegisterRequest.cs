using Encore.Messaging;

namespace Encore.Messages.Requests;

public class ChannelRegisterRequest : IMessage
{
    public static Enum Command => ServerCommand.ChannelRegister;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<ChannelEntry> Channels { get; init; }

    public class ChannelEntry : SubMessage
    {
        [MessageField(order: 0)]
        public ushort ServerId { get; init; }

        [MessageField(order: 1)]
        public ushort ChannelId { get; init; }

        [MessageField(order: 2)]
        public int Port { get; init; }

        [MessageField(order: 3)]
        public int Capacity { get; init; }

        [MessageField(order: 4)]
        public bool Active { get; init; }
    }
}
