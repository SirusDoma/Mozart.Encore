using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class WaitingUserMessageResponse : IMessage
{
    public static Enum Command => ResponseCommand.WaitingUserMessage;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [CollectionMessageField(order: 1)]
    public required byte[] Content { get; init; }
}
