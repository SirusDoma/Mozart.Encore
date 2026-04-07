using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Events;

public class SetRoomAlbumRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomAlbum;
}
