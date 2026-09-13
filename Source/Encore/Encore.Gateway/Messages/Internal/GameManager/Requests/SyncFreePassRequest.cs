using Encore.Messaging;

namespace Encore.Messages.Requests;

public class SyncFreePassRequest : IMessage
{
    public static Enum Command => ServerCommand.SyncFreePass;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }
}
