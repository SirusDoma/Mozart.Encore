using Encore.Messaging;

namespace Mozart.Messages.Events;

public class RoomForceRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomForceRemoved;
}