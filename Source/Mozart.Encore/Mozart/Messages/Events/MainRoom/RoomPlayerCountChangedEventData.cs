using Encore.Messaging;

namespace Mozart;

public class RoomPlayerCountChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomPlayerCountChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [MessageField(order: 2)]
    public byte MaxPlayerCount { get; init; }

    [MessageField(order: 3)]
    public byte PlayerCount { get; init; }
}