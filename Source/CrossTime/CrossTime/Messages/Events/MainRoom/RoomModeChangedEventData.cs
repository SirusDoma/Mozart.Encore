using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Events;

public class RoomModeChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomModeChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }

    [MessageField(order: 2)]
    public GameMode Mode { get; init; }

    [MessageField(order: 3)]
    public bool HasPassword { get; init; }

    [MessageField(order: 4)]
    public bool TeamDisabled { get; init; }
}
