using Encore.Messaging;

namespace Mozart;

public class MusicLoadedEventData : IMessage
{
    public static Enum Command => EventCommand.MusicLoaded;

    [MessageField]
    public byte MemberId { get; init; }
}