using Encore.Messaging;

namespace Encore.Messages.Responses;

public class BonusPresentItemResponse : IMessage
{
    public static Enum Command => GameManagerCommand.BonusPresentItem;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<GiftItemEntry> ItemGiftBox { get; init; } = [];

    public class GiftItemEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int GiftId { get; init; }

        [MessageField(order: 1)]
        public int ItemId { get; init; }

        [StringMessageField(order: 2)]
        public string SenderNickname { get; init; } = string.Empty;
    }
}
