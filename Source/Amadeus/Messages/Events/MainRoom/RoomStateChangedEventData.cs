using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Events;

public class RoomStateChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomStateChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [MessageField(order: 1)]
    public RoomState State { get; init; }
}