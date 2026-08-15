using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class SyncPointResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncPoint;

    [MessageField(order: 0)]
    public int Point { get; init; }
}
