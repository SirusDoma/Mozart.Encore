using Encore.Messaging;

namespace Mozart.Messages.Events;

public class UserLeaveGameEventData : IMessage
{
    public static Enum Command => EventCommand.UserLeaveGame;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public int Level { get; init; }
}