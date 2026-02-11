using Encore.Messaging;

namespace Mozart.Messages.Events;

public class MemberReadyStateChangedEventData : IMessage
{
    public static Enum Command => EventCommand.UserReadyStateChanged;

    [MessageField(order: 0 )]
    public byte MemberId { get; init; }

    [MessageField(order: 1 )]
    public bool Ready { get; init; }
}
