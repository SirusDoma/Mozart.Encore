using Encore.Messaging;

namespace Mozart;

public class PlayerTeamChangedEventData : IMessage
{
    public static Enum Command => EventCommand.PlayerTeamChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public RoomTeam Team { get; init; }
}