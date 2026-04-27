using Encore.Messaging;
using Mozart.Metadata;

namespace Identity.Messages.Events;

public class MusicLoadedEventData : IMessage
{
    public static Enum Command => EventCommand.MusicLoaded;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public PlayingState PlayingState { get; init; } = PlayingState.Playing;
}
