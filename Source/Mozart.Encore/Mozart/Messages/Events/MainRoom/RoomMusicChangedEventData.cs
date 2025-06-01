using Encore.Messaging;

namespace Mozart;

public class RoomMusicChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomMusicChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [MessageField(order: 1)]
    public int MusicId { get; init; }

    [MessageField(order: 2)]
    public Difficulty Difficulty { get; init; }

    [MessageField(order: 3)]
    public GameSpeed Speed { get; init; }
}