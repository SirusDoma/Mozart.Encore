using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class SetRoomArenaRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomArena;

    [MessageField(order: 0)]
    public required RoomArenaMessage Payload { get; init; }
}
