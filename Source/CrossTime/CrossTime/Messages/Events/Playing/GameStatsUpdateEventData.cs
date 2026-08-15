using Encore.Messaging;
using Encore.Metadata;

namespace CrossTime.Messages.Events;

public class GameStatsUpdateEventData : IMessage
{
    public static Enum Command => EventCommand.GameStatsUpdate;

    public class MemberScore : SubMessage
    {
        [MessageField(order: 0)]
        public byte MemberId { get; init; }

        [MessageField(order: 1)]
        public int Score { get; init; }
    }

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public GameUpdateStatsType Type { get; init; }

    [MessageField(order: 2)]
    public ushort Value { get; init; }

    [CollectionMessageField(order: 3, minCount: 8, maxCount: 8)]
    public required IReadOnlyList<MemberScore> MemberScores { get; init; }
}
