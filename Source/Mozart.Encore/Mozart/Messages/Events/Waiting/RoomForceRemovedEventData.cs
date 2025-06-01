using Encore.Messaging;

namespace Mozart;

public class RoomForceRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomForceRemoved;
}