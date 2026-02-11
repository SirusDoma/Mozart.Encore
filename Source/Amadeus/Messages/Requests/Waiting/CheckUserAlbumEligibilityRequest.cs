using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class CheckUserAlbumEligibilityRequest : IMessage
{
    public static Enum Command => RequestCommand.CheckRoomUserAlbum;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}
