using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class SelectMusicStartedEventData : IMessage
{
    public static Enum Command => EventCommand.SelectMusicStarted;
}
