using Encore.Messaging;
using Mozart.Metadata.Items;

namespace Amadeus.Messages.Responses;

public class EquipItemResponse : IMessage
{
    public static Enum Command => ResponseCommand.EquipItem;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public ItemType ItemType { get; init; }

    [MessageField(order: 2)]
    public int NewEquippedItemId { get; init; }

    [MessageField(order: 3)]
    public int InventorySlotIndex { get; init; }

    [MessageField(order: 4)]
    public int PreviousEquippedItemId { get; init; }
}