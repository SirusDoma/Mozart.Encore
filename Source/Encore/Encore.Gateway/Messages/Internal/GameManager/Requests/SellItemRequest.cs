using Encore.Messaging;

namespace Encore.Messages.Requests;

public class SellItemRequest : IMessage
{
    public static Enum Command => ServerCommand.SellItem;

    [MessageField(order: 0)]
    public int UserId { get; init; }

    [MessageField(order: 1)]
    public int InventorySlotIndex { get; init; }

    [MessageField(order: 2)]
    public int ItemId { get; init; }

    [MessageField(order: 3)]
    public int Gem { get; init; }
}
