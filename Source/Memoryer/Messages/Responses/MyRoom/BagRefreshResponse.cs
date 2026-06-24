using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class BagRefreshResponse : IMessage
{
    public static Enum Command => ResponseCommand.BagRefresh;

    public class AttributiveItemInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int AttributiveItemId { get; init; }

        [MessageField(order: 1)]
        public int ItemCount { get; init; }
    }

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public IList<int> Inventory { get; init; } = [];

    [CollectionMessageField(order: 2, prefixSizeType: TypeCode.Int32)]
    public IList<AttributiveItemInfo> AttributiveItems { get; init; } = [];
}
