using Encore.Messaging;

namespace Mozart;

public class GameCompletedEventData : IMessage
{
    public static Enum Command => EventCommand.GameCompleted;

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
        public ushort? Reward { get; init; }

        [MessageField(order: 10)]
        public int? Level { get; init; }

        [MessageField(order: 11)]
        public int? Experience { get; init; }

        [MessageField(order: 12)]
        public bool Win { get; init; }

        [MessageField(order: 13)]
        public bool Safe { get; init; }

    }

    [CollectionMessageField(order: 0, minCount: 8, maxCount: 8, prefixSizeType: TypeCode.Int32)]
    public required IList<ScoreEntry> Scores { get; init; } = [];
}