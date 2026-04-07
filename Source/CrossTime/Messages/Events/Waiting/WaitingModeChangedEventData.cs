using Encore.Messaging;
using Mozart.Metadata;

namespace CrossTime.Messages.Events;

public class WaitingModeChangedEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingModeChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [StringMessageField(order: 1)]
    public required string Title { get; init; }

    [MessageField(order: 2)]
    public GameMode Mode { get; init; } = GameMode.Versus;

    [MessageField(order: 7)]
    public bool HasPassword { get; init; }

    [StringMessageField(order: 4)]
    public required string Password { get; init; }

    [MessageField(order: 5)]
    public bool TeamDisabled { get; init; }
}
