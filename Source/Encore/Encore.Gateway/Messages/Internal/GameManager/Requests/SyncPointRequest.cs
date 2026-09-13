using Encore.Messaging;

namespace Encore.Messages.Requests;

public class SyncPointRequest : IMessage
{
    public static Enum Command => ServerCommand.SyncPoint;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }
}
