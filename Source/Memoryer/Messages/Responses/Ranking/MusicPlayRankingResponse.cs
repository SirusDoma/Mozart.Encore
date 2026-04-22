using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Responses;

public class MusicPlayRankingResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetMusicPlayRanking;

    public class RankEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int Rank { get; init; }

        [StringMessageField(order: 1)]
        public required string Nickname { get; init; }

        [MessageField(order: 2)]
        public int Battles { get; init; }

        [MessageField(order: 3)]
        public int Wins { get; init; }

        [MessageField(order: 4)]
        public int WinRate { get; init; }

        [MessageField(order: 5)]
        public RankDeltaType RankDeltaType { get; init; }

        [MessageField(order: 4)]
        public int RankDelta { get; init; }
    }

    [MessageField(order: 0)]
    public required RankEntry Self { get; init; }

    [MessageField(order: 1)]
    public IList<RankEntry> Entries { get; init; } = [];
}
