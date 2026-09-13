using Encore.Messaging;
using Encore.Metadata;

namespace Encore.Messages.Requests;

public class ClaimGiftRequest : IMessage
{
    public static Enum Command => ServerCommand.ClaimGift;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }

    [MessageField(order: 2)]
    public GiftType GiftType { get; init; }

    [MessageField(order: 3)]
    public int GiftId { get; init; }

    [MessageField(order: 4)]
    public byte InventorySlotIndex { get; init; }
}
