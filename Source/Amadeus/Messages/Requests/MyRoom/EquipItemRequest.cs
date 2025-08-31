using Encore.Messaging;
using Mozart.Metadata.Items;

namespace Amadeus.Messages.Requests;

public class EquipItemRequest : IMessage
{
    public static Enum Command => RequestCommand.EquipItem;

    [MessageField(order: 0)]
    public ItemType ItemType { get; init; }

    [MessageField(order: 1)]
    public int InventorySlotIndex { get; init; }
}