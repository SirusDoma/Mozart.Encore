using Encore.Messaging;

namespace Mozart;

public enum StatsType : ushort
{
    Health = 0x0000,
    Jam    = 0x0001
}

public class UpdateGameStatsRequest : IMessage
{
    public static Enum Command => RequestCommand.UpdateGameStats;

    [MessageField(order: 0)]
    public StatsType Type { get; init; }

    [MessageField(order: 1)]
    public ushort Value { get; init; }
}