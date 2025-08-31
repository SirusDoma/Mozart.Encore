using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class RoomUserCountChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomUserCountChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [MessageField(order: 2)]
    public byte Capacity { get; init; }

    [MessageField(order: 3)]
    public byte UserCount { get; init; }
}