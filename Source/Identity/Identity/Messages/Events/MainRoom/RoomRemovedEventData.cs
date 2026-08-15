using Encore.Messaging;

namespace Identity.Messages.Events;

public class RoomRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomRemoved;

    [MessageField(order: 0)]
    public int Number { get; init; }
}
