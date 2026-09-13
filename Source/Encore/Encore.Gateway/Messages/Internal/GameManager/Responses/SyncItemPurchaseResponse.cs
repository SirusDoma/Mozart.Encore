using Encore.Messaging;

namespace Encore.Messages.Responses;

public class SyncItemPurchaseResponse : IMessage
{
    public static Enum Command => GameManagerCommand.SyncItemPurchase;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int Gem { get; init; }

    [MessageField(order: 2)]
    public int Point { get; init; }

    [MessageField(order: 3)]
    public int O2Cash { get; init; }

    [CollectionMessageField(order: 4, minCount: 30, maxCount: 30)]
    public IReadOnlyList<int> Inventory { get; init; } = [];

    [MessageField(order: 5)]
    public int MusicCash { get; init; }

    [MessageField(order: 6)]
    public int ItemCash { get; init; }

    [CollectionMessageField(order: 7, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<AttributiveItemEntry> AttributiveItems { get; init; } = [];

    public class AttributiveItemEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int ItemId { get; init; }

        [MessageField(order: 1)]
        public int Count { get; init; }
    }
}
