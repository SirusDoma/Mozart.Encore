using Encore.Messaging;

namespace Identity.Messages.Events;

public class KickEventData : IMessage
{
    public static Enum Command => EventCommand.Kick;
}
