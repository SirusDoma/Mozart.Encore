using Encore.Messaging;

namespace Encore.Messages.Requests;

public class EquipItemRequest : IMessage
{
    public static Enum Command => ServerCommand.EquipItem;

    [MessageField(order: 0)]
    public int UserId { get; init; }

    [MessageField(order: 1)]
    public int EquipmentSlotIndex { get; init; }

    [MessageField(order: 2)]
    public int InventorySlotIndex { get; init; }
}
