using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class ReadGiftMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.ReadGiftMessage;

    [MessageField(order: 0)]
    public required int GiftMessageId { get; init; }
}
