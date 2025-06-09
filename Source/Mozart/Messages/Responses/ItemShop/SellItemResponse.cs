using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class SellItemResponse : IMessage
{
    public static Enum Command => ResponseCommand.SellItem;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public int TotalUserGem { get; init; }

    [MessageField(order: 2)]
    public int TotalUserPoint { get; init; }

    [MessageField(order: 3)]
    public int InventorySlotIndex { get; init; }
}