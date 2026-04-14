using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class JoinRoomRequest : IMessage
{
    public static Enum Command => RequestCommand.JoinWaiting;

    [MessageField(order: 0)]
    public int RoomNumber { get; init; }

    [MessageField(order: 1)]
    public MusicState MusicState { get; init; }

    [StringMessageField(order: 2)]
    public string Password { get; init; } = "\0\0\0\0";
}
