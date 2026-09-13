using Encore.Messaging;

namespace Encore.Messages.Responses;

public class AttributiveItemListResponse : IMessage
{
    public static Enum Command => GameManagerCommand.AttributiveItemList;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<AttributiveItemEntry> AttributiveItems { get; init; } = [];

    public class AttributiveItemEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int ItemId { get; init; }

        [MessageField(order: 1)]
        public int Count { get; init; }
    }
}
