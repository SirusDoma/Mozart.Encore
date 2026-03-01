using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Responses;

public class MissionRanksResponse : IMessage
{
    public static Enum Command => ResponseCommand.MissionRanks;

    public class RankEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int MissionLevel { get; init; }

        [MessageField(order: 1)]
        public Rank Rank { get; init; }
    }

    [MessageField(order: 0)]
    public int MissionSetId { get; init; } = 1; // Active?

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<RankEntry> Ranks { get; init; } = [];
}
