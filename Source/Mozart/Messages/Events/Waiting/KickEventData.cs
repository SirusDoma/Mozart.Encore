using Encore.Messaging;

namespace Mozart.Messages.Events;

public class KickEventData : IMessage
{
    public static Enum Command => EventCommand.Kick;
}
