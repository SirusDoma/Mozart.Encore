using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class JoinRoomRequest : IMessage
{
    public static Enum Command => RequestCommand.JoinWaiting;

    [MessageField(order: 0)]
    public int RoomNumber { get; init; }

    [StringMessageField(order: 1)]
    public string Password { get; init; } = "\0\0\0\0";
}