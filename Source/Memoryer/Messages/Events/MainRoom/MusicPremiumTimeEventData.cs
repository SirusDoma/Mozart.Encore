using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class MusicPremiumTimeEventData : IMessage
{
    public static Enum Command => EventCommand.MusicPremiumTimeList;

    public class MusicEntry : SubMessage
    {
        [MessageField(order: 0)]
        public ushort MusicId { get; init; }

        [MessageField(order: 1)]
        public byte Day { get; init; }

        [MessageField(order: 2)]
        public byte Month { get; set; }

        [MessageField(order: 3)]
        public byte Year { get; set; }

        [MessageField(order: 4)]
        public byte Hour { get; set; }

        [MessageField(order: 5)]
        public byte Minute { get; set; }
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<MusicEntry> Entries { get; init; } = [];
}
