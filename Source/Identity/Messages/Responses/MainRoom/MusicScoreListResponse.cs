using Encore.Messaging;

namespace Identity.Messages.Responses;

public class MusicScoreListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetMusicScoreList;

    public class MusicScoreEntry : SubMessage
    {
        [MessageField(order: 0)]
        public short MusicId { get; init; }

        [MessageField(order: 1)]
        private short Unused1 { get; init; } // Unused

        // TODO: Use Dictionary for Scores and Rank with Difficulty as the key.
        //       (Write a new codec for it)

        [CollectionMessageField(order: 2, minCount: 3, maxCount: 3, prefixSizeType: TypeCode.Empty)]
        public required IReadOnlyList<int> Scores { get; init; }

        [CollectionMessageField(order: 3, minCount: 3, maxCount: 3, prefixSizeType: TypeCode.Empty)]
        public required IReadOnlyList<byte> Ranks { get; init; }

        [MessageField(order: 4)]
        private byte Unused2 { get; init; } // Unused
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Empty)]
    public required IReadOnlyList<MusicScoreEntry> MaxScores { get; init; }
}
