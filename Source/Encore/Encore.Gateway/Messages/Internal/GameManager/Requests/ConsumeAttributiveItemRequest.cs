using Encore.Messaging;

namespace Encore.Messages.Requests;

public class ConsumeAttributiveItemRequest : IMessage
{
    public static Enum Command => ServerCommand.ConsumeAttributiveItem;

    [MessageField(order: 0)]
    public int UserId { get; init; }

    [MessageField(order: 1)]
    public int ItemId { get; init; }
}
