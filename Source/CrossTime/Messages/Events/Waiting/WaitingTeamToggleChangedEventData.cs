using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Events;

public class WaitingTeamToggleChangedEventData : IMessage
{
    public static Enum Command => EventCommand.ToggleTeamMode;

    public class MemberTeamInfo : SubMessage
    {
        [MessageField(order: 0)]
        public byte MemberId { get; init; }

        [MessageField(order: 1)]
        public RoomTeam Team { get; init; }
    }

    [MessageField(order: 0)]
    public bool Disabled { get; init; }

    [CollectionMessageField(order: 1, minCount: 8, maxCount: 8)]
    public IReadOnlyList<MemberTeamInfo> Teams { get; init; } = [];
}
