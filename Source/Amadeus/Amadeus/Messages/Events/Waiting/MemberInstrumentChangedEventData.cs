using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class MemberInstrumentChangedEventData : IMessage
{
    public static Enum Command => EventCommand.UserInstrumentChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public int InstrumentId { get; init; }
}
