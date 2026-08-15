using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class SetRoomAlbumRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomAlbum;
}
