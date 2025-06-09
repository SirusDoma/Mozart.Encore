using Encore.Messaging;

namespace Mozart.Messages.Events;

public class WaitingRoomTitleEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingTitleChanged;

    [StringMessageField(maxLength: 21)]
    public required string Title { get; init; }
}