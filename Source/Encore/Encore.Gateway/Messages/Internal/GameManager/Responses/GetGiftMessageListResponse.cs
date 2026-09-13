using Encore.Messaging;
using Encore.Metadata;

namespace Encore.Messages.Responses;

public class GetGiftMessageListResponse : IMessage
{
    public static Enum Command => GameManagerCommand.GetGiftMessageList;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.UInt16)]
    public IReadOnlyList<GiftEntry> Messages { get; init; } = [];

    public class GiftEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int MessageId { get; init; }

        [MessageField(order: 1)]
        public GiftType GiftType { get; init; }

        [StringMessageField(order: 2)]
        public string WriteDate { get; init; } = string.Empty;

        [StringMessageField(order: 3)]
        public string SenderNickname { get; init; } = string.Empty;

        [StringMessageField(order: 4)]
        public string Title { get; init; } = string.Empty;

        [StringMessageField(order: 5)]
        public string Content { get; init; } = string.Empty;
    }
}
