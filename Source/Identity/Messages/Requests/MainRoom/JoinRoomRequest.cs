using Encore.Messaging;
using Mozart.Metadata;

namespace Identity.Messages.Requests;

public class JoinRoomRequest : IMessage
{
    public static Enum Command => RequestCommand.JoinWaiting;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [MessageField(order: 1)]
    public AlbumState AlbumState { get; init; }

    [StringMessageField(order: 2)]
    public string Password { get; init; } = "\0\0\0\0";
}
