using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class SelectMusicCancelledEventData : IMessage
{
    public static Enum Command => EventCommand.SelectMusicCancelled;
}
