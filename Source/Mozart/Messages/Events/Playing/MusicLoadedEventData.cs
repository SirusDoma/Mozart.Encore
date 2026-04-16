using Encore.Messaging;

namespace Mozart.Messages.Events;

public class MusicLoadedEventData : IMessage
{
    public static Enum Command => EventCommand.MusicLoaded;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public bool IsPlaying { get; init; } = true;
}
