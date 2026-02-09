using Encore.Messaging;
using Mozart.Data.Entities;

namespace Amadeus.Messages.Responses;

public class SyncPurchaseResponse : IMessage
{
    public class AttributiveItemInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int AttributiveItemId { get; init; }

        [MessageField(order: 1)]
        public int ItemCount { get; init; }
    }

    public static Enum Command => ResponseCommand.SyncPurchase;

    [MessageField(order: 0)]
    public int Gem { get; init; }

    [MessageField(order: 1)]
    public int Point { get; init; }

    [MessageField(order: 2)]
    public int O2Cash { get; init; }

    [CollectionMessageField(order: 3, minCount: 30, maxCount:30)]
    public IReadOnlyList<int> Inventory { get; init; } = [];

    [MessageField(order: 4)]
    public int ItemCash { get; init; }

    [MessageField(order: 5)]
    public int MusicCash { get; init; }

    [CollectionMessageField(order: 6, prefixSizeType: TypeCode.Int32)]
    public IList<AttributiveItemInfo> AttributiveItems { get; init; } = [];
}