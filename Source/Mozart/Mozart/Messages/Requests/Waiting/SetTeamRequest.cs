using Encore.Messaging;
using Encore.Metadata;

namespace Mozart.Messages.Requests;

public class SetTeamRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomTeam;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public RoomTeam Team { get; init; }
}
