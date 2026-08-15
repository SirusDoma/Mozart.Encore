using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class RoomForceRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomForceRemoved;
}
