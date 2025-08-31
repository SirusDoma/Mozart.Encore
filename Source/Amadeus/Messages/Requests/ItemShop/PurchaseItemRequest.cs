using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class PurchaseItemRequest : IMessage
{
    public static Enum Command => RequestCommand.PurchaseItem;

    [MessageField(order: 0)]
    public int ItemId { get; init; }
}