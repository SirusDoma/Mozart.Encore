using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class MusicStateChangedEventData : IMessage
{
    public static Enum Command => EventCommand.MusicStateChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public bool Playing { get; init; }

    [MessageField(order: 2)]
    public MusicState State { get; init; }
}
