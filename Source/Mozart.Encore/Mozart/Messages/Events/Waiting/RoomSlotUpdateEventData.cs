using Encore.Messaging;

namespace Mozart;

public class RoomSlotUpdateEventData : IMessage
{
    public enum EventType : byte
    {
        SlotUnlocked = 0,
        SlotLocked   = 2,
        PlayerKicked = 3
    }

    public static Enum Command => EventCommand.RoomSlotUpdate;

    [MessageField(order: 0)]
    public byte Index { get; init; }

    [MessageField(order: 1)]
    public EventType Type { get; init; }
}