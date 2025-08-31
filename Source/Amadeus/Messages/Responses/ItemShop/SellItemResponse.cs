using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class SellItemResponse : IMessage
{
    public static Enum Command => ResponseCommand.SellItem;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public int Unknown { get; init; } // Slot index?
}