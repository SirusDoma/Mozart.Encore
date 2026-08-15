using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class RoomForceRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomForceRemoved;
}
