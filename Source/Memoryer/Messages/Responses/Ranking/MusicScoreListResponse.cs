using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Responses;

public class MusicScoreListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetMusicScoreList;

    public class MusicScoreEntry : SubMessage
    {
        [MessageField(order: 0)]
        public ushort MusicId { get; init; }

        [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Empty)]
        public required Dictionary<Difficulty, Rank> Ranks { get; init; }
    }

    [MessageField(order: 0)]
    private int Result { get; init; } = 0; // Treated as success when value >= 0

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int16)]
    public required IReadOnlyList<MusicScoreEntry> MaxScores { get; init; }
}
