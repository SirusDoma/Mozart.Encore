using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class GetWaitingStateRequest : IMessage
{
    public static Enum Command => RequestCommand.GetWaitingState;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}
