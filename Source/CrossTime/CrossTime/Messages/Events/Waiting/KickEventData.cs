using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class KickEventData : IMessage
{
    public static Enum Command => EventCommand.Kick;
}
