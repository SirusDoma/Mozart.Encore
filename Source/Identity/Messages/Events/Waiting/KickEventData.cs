using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class KickEventData : IMessage
{
    public static Enum Command => EventCommand.Kick;
}
