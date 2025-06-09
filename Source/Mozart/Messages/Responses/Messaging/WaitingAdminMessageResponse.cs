using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class WaitingAdminMessageResponse : IMessage
{
    public static Enum Command => ResponseCommand.WaitingAdminMessage;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [StringMessageField(order: 1)]
    public required string Content { get; init; }
}