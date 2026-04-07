using Encore.Messaging;

namespace Identity.Messages.Requests;

public class GiftFreePassRequest : IMessage
{
    public static Enum Command =>  RequestCommand.GiftFreePass;

    [StringMessageField(order: 0)]
    public required string Recipient { get; init; }

    [StringMessageField(order: 1)]
    public required string Sender { get; init; }
}
