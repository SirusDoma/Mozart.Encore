using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class KickEventData : IMessage
{
    public static Enum Command => EventCommand.Kick;
}