using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Events;

public class ScoreCompletedEventData : IMessage
{
    public static Enum Command => EventCommand.ScoreCompleted;

    public class ScoreEntry : SubMessage
    {
        [MessageField(order: 0)]
        public byte MemberId { get; init; }

        [MessageField<MessageFieldCodec<int>>(order: 1)]
        public bool Active { get; init; } = false;

        [MessageField(order: 2)]
        public ushort? Cool { get; init; }

        [MessageField(order: 3)]
        public ushort? Good { get; init; }

        [MessageField(order: 4)]
        public ushort? Bad { get; init; }

        [MessageField(order: 5)]
        public ushort? Miss { get; init; }

        [MessageField(order: 6)]
        public ushort? MaxCombo { get; init; }

        [MessageField(order: 7)]
        public ushort? JamCombo { get; init; }

        [MessageField(order: 8)]
        public uint? Score { get; init; }

        [MessageField(order: 9)]
        public int? Gem { get; init; }

        [MessageField(order: 10)]
        public int? Level { get; init; }

        [MessageField(order: 11)]
        public int? Experience { get; init; }

        [MessageField(order: 12)]
        public bool? Win { get; init; }

        [MessageField(order: 13)]
        public bool? Draw { get; init; }

        [MessageField(order: 14)]
        public int? RewardGemStar { get; init; }

        [MessageField(order: 15)]
        public GameSpeed? Speed { get; init; }

        [MessageField(order: 16)]
        public int? Penalty { get; init; }
    }

    [CollectionMessageField(order: 0, maxCount: 8, prefixSizeType: TypeCode.Int32)]
    public required IList<ScoreEntry> Scores { get; init; } = [];
}
