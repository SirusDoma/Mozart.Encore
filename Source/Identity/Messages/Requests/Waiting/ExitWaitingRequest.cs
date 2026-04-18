using Encore.Messaging;

namespace Identity.Messages.Requests;

public class ExitWaitingRequest : IMessage
{
    public static Enum Command => RequestCommand.ExitWaiting;

    [MessageField(order: 0)]
    public short Reason { get; init; }
}
