using Encore.Messaging;

namespace Identity.Messages.Requests;

public class ReadGiftMessageRequest : IMessage
{
    public static Enum Command => RequestCommand.ReadGiftMessage;

    [MessageField(order: 0)]
    public required int GiftMessageId { get; init; }
}
