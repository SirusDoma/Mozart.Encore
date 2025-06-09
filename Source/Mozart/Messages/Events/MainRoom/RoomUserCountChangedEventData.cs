using Encore.Messaging;

namespace Mozart.Messages.Events;

public class RoomUserCountChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomPlayerCountChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [MessageField(order: 2)]
    public byte Capacity { get; init; }

    [MessageField(order: 3)]
    public byte UserCount { get; init; }
}