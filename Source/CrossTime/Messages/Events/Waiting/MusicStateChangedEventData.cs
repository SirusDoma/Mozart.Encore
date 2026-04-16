using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Responses;

public class MusicStateChangedEventData : IMessage
{
    public static Enum Command => EventCommand.MusicStateChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public MusicState State { get; init; } = MusicState.None;
}
