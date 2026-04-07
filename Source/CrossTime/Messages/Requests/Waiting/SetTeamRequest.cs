using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Requests;

public class SetTeamRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomTeam;

    [MessageField(order: 0)]
    public RoomTeam Team { get; init; }
}
