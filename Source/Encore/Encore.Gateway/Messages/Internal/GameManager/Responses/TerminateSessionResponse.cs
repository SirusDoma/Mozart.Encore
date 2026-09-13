using Encore.Messaging;

namespace Encore.Messages.Responses;

public class TerminateSessionResponse : IMessage
{
    public static Enum Command => GameManagerCommand.TerminateSession;

    [MessageField(order: 0)]
    public int UserId { get; init; }

    [MessageField(order: 1)]
    public uint SessionId { get; init; }
}
