using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Events;

public class GameStatsUpdateEventData : IMessage
{
    public static Enum Command => EventCommand.GameStatsUpdate;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public GameUpdateStatsType Type { get; init; }

    [MessageField(order: 2)]
    public ushort Value { get; init; }
}
