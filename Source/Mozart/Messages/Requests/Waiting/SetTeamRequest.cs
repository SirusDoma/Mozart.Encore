using Encore.Messaging;
using Mozart.Metadata;

namespace Mozart.Messages.Requests;

public class SetTeamRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomTeam;

    [MessageField<MessageFieldCodec<int>>(order: 0)] // Likely to be a human-error, but we have to deal with it
    public RoomTeam Team { get; init; }
}
