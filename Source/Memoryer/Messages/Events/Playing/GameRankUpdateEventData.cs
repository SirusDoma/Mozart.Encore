using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class GameRankUpdateEventData : IMessage
{
    public static Enum Command => EventCommand.GameRankUpdate;

    [CollectionMessageField(order: 0, minCount: 8, maxCount: 8)]
    public required IReadOnlyList<byte> MemberRanks { get; init; }
}
