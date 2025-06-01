using Encore.Messaging;

namespace Mozart;

public class SetInstrumentRequest : IMessage
{
    public static Enum Command => RequestCommand.SetInstrument;

    [MessageField(order: 0)]
    public int InstrumentId { get; init; }
}