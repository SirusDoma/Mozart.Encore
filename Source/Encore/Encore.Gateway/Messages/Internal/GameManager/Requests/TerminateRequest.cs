using Encore.Messaging;

namespace Encore.Messages.Requests;

public class TerminateRequest : IMessage
{
    public static Enum Command => ServerCommand.TerminateSession;

    [MessageField(order: 0)]
    public int UserId { get; init; }

    [MessageField(order: 1)]
    public uint SessionId { get; init; }
}
