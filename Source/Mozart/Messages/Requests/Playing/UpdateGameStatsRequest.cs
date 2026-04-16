using Encore.Messaging;
using Mozart.Metadata;

namespace Mozart.Messages.Requests;

public class UpdateGameStatsRequest : IMessage
{
    public static Enum Command => RequestCommand.UpdateGameStats;

    [MessageField(order: 0)]
    public GameUpdateStatsType Type { get; init; }

    [MessageField(order: 1)]
    public ushort Value { get; init; }

    [MessageField(order: 2)]
    public int Sequence { get; init; }

    [MessageField(order: 3)]
    public uint Score { get; init; }

    [MessageField(order: 4)]
    public int LongNoteScore { get; init; }
}
