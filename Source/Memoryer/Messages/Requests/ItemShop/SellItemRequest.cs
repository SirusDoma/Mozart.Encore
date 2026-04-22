using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class SellItemRequest : IMessage
{
    public static Enum Command => RequestCommand.SellItem;

    [MessageField(order: 0)]
    public int InventorySlotIndex { get; init; }

    [MessageField(order: 1)]
    public int ItemId { get; init; }
}
