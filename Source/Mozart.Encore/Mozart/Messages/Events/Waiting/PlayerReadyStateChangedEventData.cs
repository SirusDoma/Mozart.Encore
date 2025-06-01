using Encore.Messaging;

namespace Mozart;

public class PlayerReadyStateChangedEventData : IMessage
{
    public static Enum Command => EventCommand.PlayerReadyStateChanged;

    [MessageField(order: 0 )]
    public byte MemberId { get; init; }

    [MessageField(order: 1 )]
    public bool Ready { get; init; }
}