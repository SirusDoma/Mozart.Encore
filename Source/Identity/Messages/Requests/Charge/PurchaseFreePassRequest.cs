using Encore.Messaging;

namespace Identity.Messages.Requests;

public class PurchaseFreePassRequest : IMessage
{
    public static Enum Command =>  RequestCommand.PurchaseFreePass;

    [StringMessageField(order: 0)]
    public required string Username { get; init; }
}
