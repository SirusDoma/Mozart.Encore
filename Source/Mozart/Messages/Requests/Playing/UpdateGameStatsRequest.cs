using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class UpdateGameStatsRequest : IMessage
{
    public static Enum Command => RequestCommand.UpdateGameStats;

    [MessageField(order: 0)]
    public GameUpdateStatsType Type { get; init; }

    [MessageField(order: 1)]
    public ushort Value { get; init; }
}