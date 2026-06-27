using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class MemberStateChangedEventData : IMessage
{
    public static Enum Command => EventCommand.MemberStateChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public PlayingState PlayingState { get; init; }

    [MessageField(order: 2)]
    public MusicState MusicState { get; init; }
}
