using Encore.Messaging;

namespace Encore.Messages.Requests;

public class UserStatsUpdateRequest : IMessage
{
    public static Enum Command => ServerCommand.UserStatsUpdate;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<UserStats> Users { get; init; } = [];

    public class UserStats : SubMessage
    {
        [MessageField(order: 0)]
        public int UserId { get; init; }

        [MessageField(order: 1)]
        public int Gem { get; init; }

        [MessageField(order: 2)]
        public int Level { get; init; }

        [MessageField(order: 3)]
        public int Battle { get; init; }

        [MessageField(order: 4)]
        public int Win { get; init; }

        [MessageField(order: 5)]
        public int Draw { get; init; }

        [MessageField(order: 6)]
        public int Lose { get; init; }

        [MessageField(order: 7)]
        public int Experience { get; init; }

        [MessageField(order: 8)]
        public int BonusPoint { get; init; }

        [MessageField(order: 9)]
        public int Point { get; init; }

        [MessageField(order: 10)]
        public int O2Cash { get; init; }
    }
}
