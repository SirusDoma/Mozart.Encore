using Encore.Messaging;

namespace Amadeus.Messages.Responses;

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

        [MessageField(order: 4)]
        public int Unknown { get; init; } = 0;
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int16)]
    public required IReadOnlyList<MusicInfo> MusicList { get; init; }
}