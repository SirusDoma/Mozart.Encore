using Encore.Messaging;

namespace Mozart.Messages.Events;

public class WaitingMusicChangedEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingMusicChanged;

    [MessageField(order: 0)]
    public int MusicId { get; init; }

    [MessageField(order: 1)]
    public Difficulty Difficulty { get; init; }

    [MessageField(order: 2)]
    public GameSpeed Speed { get; init; }
}