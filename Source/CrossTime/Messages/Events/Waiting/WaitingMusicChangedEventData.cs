using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class WaitingMusicChangedEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingMusicChanged;

    [MessageField(order: 0)]
    public ushort MusicId { get; init; }

    [MessageField(order: 1)]
    public byte MissionLevel { get; init; }
}
