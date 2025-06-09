using Encore.Messaging;

namespace Mozart.Messages.Events;

public class RoomTitleChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomTitleChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }
}