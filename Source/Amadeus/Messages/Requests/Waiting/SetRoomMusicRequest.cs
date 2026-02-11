using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Requests;

public class SetRoomMusicRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomMusic;

    [MessageField(order: 0)]
    public ushort MusicId { get; init; }

    [MessageField(order: 1)]
    public Difficulty Difficulty { get; init; }

    [MessageField(order: 2)]
    public GameSpeed Speed { get; init; }
}
