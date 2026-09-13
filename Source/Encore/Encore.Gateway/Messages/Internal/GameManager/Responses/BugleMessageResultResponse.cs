using Encore.Messaging;

namespace Encore.Messages.Responses;

public class BugleMessageResultResponse : IMessage
{
    public static Enum Command => GameManagerCommand.BugleMessageResult;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int Result { get; init; }

    [MessageField(order: 2)]
    public int WaitTime { get; init; }
}
