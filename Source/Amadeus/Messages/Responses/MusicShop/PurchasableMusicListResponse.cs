using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class PurchasableMusicListResponse : IMessage
{
    public static Enum Command => ResponseCommand.PurchasableMusic;

    public class MusicItem : SubMessage
    {
        [MessageField(order: 0)]
        public int MusicId { get; init; }

        [MessageField(order: 1)]
        public byte Flag { get; init; }

        [StringMessageField(order: 2)]
        public required string Title { get; init; }

        [StringMessageField(order: 3)]
        public required string Artist { get; init; }

        [StringMessageField(order: 4)]
        public required string NoteDesigner { get; init; }

        // This could be old genre instead
        [StringMessageField(order: 5)]
        public required string OJM { get; init; }
    }

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [CollectionMessageField(order:1, prefixSizeType: TypeCode.Int16)]
    public IReadOnlyList<MusicItem> Items { get; init; } = [];
}