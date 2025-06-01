using Encore.Messaging;

namespace Mozart;

public class PlayerInstrumentChangedEventData : IMessage
{
    public static Enum Command => EventCommand.PlayerInstrumentChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public int InstrumentId { get; init; }
}