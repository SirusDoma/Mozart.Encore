using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class InvalidateCharacterInfoEventData : IMessage
{
    public static Enum Command => EventCommand.InvalidateCharacterInfo;
}
