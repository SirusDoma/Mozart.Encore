using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class PurchaseItemResponse : IMessage
{
    public enum PurchaseResult : int
    {
        Success           = 0x00000000, // 0
        InsufficientMoney = 0x00000001, // 1
        InventoryFull     = 0x00000002  // 2
    }

    public static Enum Command => ResponseCommand.PurchaseItem;

    [MessageField(order: 0)]
    public PurchaseResult Result { get; init; } = PurchaseResult.Success;

    [MessageField(order: 1)]
    public int TotalUserGem { get; init; }

    [MessageField(order: 2)]
    public int TotalUserPoint { get; init; }

    [MessageField(order: 3)]
    public int InventorySlotIndex { get; init; }

    [MessageField(order: 4)]
    public int ItemId { get; init; }
}