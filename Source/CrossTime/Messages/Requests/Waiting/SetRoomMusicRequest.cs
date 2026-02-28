using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Requests;

public class SetRoomMusicRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomMusic;

    [MessageField(order: 0)]
    public ushort MusicId { get; init; }

    [MessageField(order: 2)]
    public byte MissionLevel { get; init; } // Bugged: the client will always send previous selected level
}
