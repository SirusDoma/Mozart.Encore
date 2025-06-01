using Encore.Messaging;

namespace Mozart;

public class KickEventData : IMessage
{
    public static Enum Command => EventCommand.Kick;
}