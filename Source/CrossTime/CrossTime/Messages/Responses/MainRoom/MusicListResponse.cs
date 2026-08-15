using CrossTime.Messages.Codecs;
using Encore.Messaging;
using Encore.Metadata;

namespace CrossTime.Messages.Responses;

public class MusicListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetMusicList;

    public class MusicInfo : SubMessage
    {
        [MessageField(order: 0)]
        public ushort Id { get; init; }

        [MessageField(order: 1)]
        public ushort NoteCountEx { get; init; }

        [MessageField(order: 2)]
        public ushort NoteCountNx { get; init; }

        [MessageField(order: 3)]
        public ushort NoteCountHx { get; init; }

        [MessageField<MissionDifficultyCodec>(order: 4)]
        public Difficulty MissionDifficulty { get; init; }

        [MessageField(order: 5)]
        public int PriceGem { get; init; }
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int16)]
    public required IReadOnlyList<MusicInfo> MusicList { get; init; }
}
