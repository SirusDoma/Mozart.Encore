using Encore.Messaging;

namespace Identity.Messages.Requests;

public class ChangeNameRequest : IMessage
{
    public static Enum Command => RequestCommand.ChangeName;

    [StringMessageField(order: 0)]
    public required string Name { get; init; }

    [MessageField(order: 1)]
    public int InventorySlotIndex { get; init; }

    [MessageField(order: 2)]
    public int ItemId { get; init; }
}
