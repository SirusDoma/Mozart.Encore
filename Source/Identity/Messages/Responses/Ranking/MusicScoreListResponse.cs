using Encore.Messaging;
using Mozart.Metadata;

namespace Identity.Messages.Responses;

public class MusicScoreListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetMusicScoreList;

    public class MusicScoreEntry : SubMessage
    {
        [MessageField(order: 0)]
        public ushort MusicId { get; init; }

        [MessageField(order: 1)]
        private short Unused1 => 0;

        [CollectionMessageField(order: 2, prefixSizeType: TypeCode.Empty)]
        public required Dictionary<Difficulty, int> Scores { get; init; }

        [CollectionMessageField(order: 3, prefixSizeType: TypeCode.Empty)]
        public required Dictionary<Difficulty, Rank> Ranks { get; init; }

        [MessageField(order: 4)]
        private byte Unused2 => 0;
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Empty)]
    public required IReadOnlyList<MusicScoreEntry> MaxScores { get; init; }
}
