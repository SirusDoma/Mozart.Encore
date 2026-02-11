using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class SyncPointResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncPoint;

    [MessageField(order: 1)]
    public int Point { get; init; }
}
