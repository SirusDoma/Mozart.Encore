using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class SellItemRequest : IMessage
{
    public static Enum Command => RequestCommand.SellItem;

    [MessageField(order: 0)]
    public int InventorySlotIndex { get; init; }
}
