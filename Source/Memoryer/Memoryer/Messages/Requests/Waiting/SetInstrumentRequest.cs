using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class SetInstrumentRequest : IMessage
{
    public static Enum Command => RequestCommand.SetInstrument;

    [MessageField(order: 0)]
    public int InstrumentId { get; init; }
}
