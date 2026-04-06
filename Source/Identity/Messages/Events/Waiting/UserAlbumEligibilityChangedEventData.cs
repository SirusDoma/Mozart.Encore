using Encore.Messaging;
using Mozart.Metadata;

namespace Identity.Messages.Responses;

public class UserAlbumEligibilityChangedEventData : IMessage
{
    public static Enum Command => EventCommand.UserAlbumEligibilityChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public AlbumState AlbumState { get; init; }
}
