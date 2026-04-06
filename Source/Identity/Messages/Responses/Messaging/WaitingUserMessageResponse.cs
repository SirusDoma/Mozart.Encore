using Encore.Messaging;

namespace Identity.Messages.Responses;

public class WaitingUserMessageResponse : IMessage
{
    public static Enum Command => ResponseCommand.WaitingUserMessage;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [CollectionMessageField(order: 1)]
    public required byte[] Content { get; init; }

    [MessageField(order: 2)]
    public byte MemberId { get; init; }
}
