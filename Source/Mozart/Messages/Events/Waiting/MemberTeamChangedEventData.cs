using Encore.Messaging;
using Mozart.Metadata;

namespace Mozart.Messages.Events;

public class MemberTeamChangedEventData : IMessage
{
    public static Enum Command => EventCommand.UserTeamChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public RoomTeam Team { get; init; }
}
