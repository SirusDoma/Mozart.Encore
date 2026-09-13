using Encore.Messaging;

namespace Encore.Messages.Requests;

public class ReadGiftMessageRequest : IMessage
{
    public static Enum Command => ServerCommand.ReadGiftMessage;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }

    [MessageField(order: 2)]
    public int GiftMessageId { get; init; }
}
