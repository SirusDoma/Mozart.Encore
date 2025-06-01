using Encore.Messaging;

namespace Mozart;

public class SetTeamRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomTeam;

    [MessageField(order: 0)]
    public RoomTeam Team { get; init; }
}