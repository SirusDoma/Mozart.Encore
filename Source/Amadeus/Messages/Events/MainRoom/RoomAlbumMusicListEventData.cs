using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class RoomAlbumMusicListEventData : IMessage
{
    public static Enum Command => EventCommand.RoomAlbumMusicList;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public List<int> MusicIds { get; init; } = [];
}
