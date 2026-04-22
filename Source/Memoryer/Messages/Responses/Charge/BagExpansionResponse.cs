using Encore.Messaging;
using Mozart.Data.Entities;

namespace Memoryer.Messages.Responses;

public class BagExpansionResponse : IMessage
{
    public static Enum Command => ResponseCommand.BagExpansion;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public int ExpansionSize { get; init; } = Inventory.SlotsPerExpansion;
}
