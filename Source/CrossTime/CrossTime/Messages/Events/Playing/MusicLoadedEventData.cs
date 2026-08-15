using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class MusicLoadedEventData : IMessage
{
    public static Enum Command => EventCommand.MusicLoaded;

    [MessageField]
    public byte MemberId { get; init; }
}
