using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class SyncItemPurchaseResponse : IMessage
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
    public int GemStar { get; init; }

    [CollectionMessageField(order: 3, minCount: 30, maxCount:30)]
    public IReadOnlyList<int> Inventory { get; init; } = [];
}
