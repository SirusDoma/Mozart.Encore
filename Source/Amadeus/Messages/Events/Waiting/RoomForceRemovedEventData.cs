using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class RoomForceRemovedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomForceRemoved;
}