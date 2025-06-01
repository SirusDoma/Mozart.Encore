using Encore.Messaging;

namespace Mozart;

public class WaitingUserMessageResponse : IMessage
{
    public static Enum Command => ResponseCommand.WaitingUserMessage;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [StringMessageField(order: 1)]
    public required string Content { get; init; }
}