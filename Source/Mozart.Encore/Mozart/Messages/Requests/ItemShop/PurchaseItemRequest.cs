using Encore.Messaging;

namespace Mozart;

public class PurchaseItemRequest : IMessage
{
    public static Enum Command => RequestCommand.PurchaseItem;

    [MessageField(order: 0)]
    public int ItemId { get; init; }
}