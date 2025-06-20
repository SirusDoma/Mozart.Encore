using Encore.Messaging;

namespace Mozart.Messages.Events;

public class RoomRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomRemoved;

    [MessageField(order: 0)]
    public int Number { get; init; }
}