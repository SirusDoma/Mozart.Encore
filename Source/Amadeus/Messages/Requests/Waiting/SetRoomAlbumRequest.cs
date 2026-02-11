using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Events;

public class SetRoomAlbumRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomAlbum;

    [MessageField(order: 0)]
    public int AlbumId { get; init; }

    [MessageField(order: 1)]
    public GameSpeed Speed { get; init; }
}
