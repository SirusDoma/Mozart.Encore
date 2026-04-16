using Encore.Messaging;

namespace Identity.Messages.Requests;

public class BagExpansionRequest : IMessage
{
    public static Enum Command =>  RequestCommand.BagExpansion;

    [MessageField(order: 0)]
    public int BagSlotIndex { get; init; }

    [MessageField(order: 1)]
    public int ItemId { get; init; }
}
