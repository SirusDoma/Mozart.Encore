using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class SellItemRequest : IMessage
{
    public static Enum Command => RequestCommand.SellItem;

    [MessageField(order: 0)]
    public int InventorySlotIndex { get; init; }
}
