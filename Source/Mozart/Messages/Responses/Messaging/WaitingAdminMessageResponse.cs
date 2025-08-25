using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class WaitingAdminMessageResponse : IMessage
{
    public static Enum Command => ResponseCommand.WaitingAdminMessage;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [CollectionMessageField(order: 1)]
    public required byte[] Content { get; init; }
}