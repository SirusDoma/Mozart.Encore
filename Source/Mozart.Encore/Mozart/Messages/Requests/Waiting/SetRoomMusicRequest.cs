using Encore.Messaging;

namespace Mozart;

public class SetRoomMusicRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomMusic;

    [MessageField(order: 0)]
    public int MusicId { get; init; }

    [MessageField(order: 1)]
    public Difficulty Difficulty { get; init; }

    [MessageField(order: 2)]
    public GameSpeed Speed { get; init; }
}