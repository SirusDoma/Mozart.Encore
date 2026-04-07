using Encore.Messaging;

namespace Identity.Messages.Events;

public class RoomForceRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomForceRemoved;
}
