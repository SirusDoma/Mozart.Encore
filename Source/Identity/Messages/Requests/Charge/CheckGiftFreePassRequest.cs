using Encore.Messaging;

namespace Identity.Messages.Requests;

public class CheckGiftFreePassRequest : IMessage
{
    public static Enum Command =>  RequestCommand.CheckGiftFreePass;

    [StringMessageField(order: 0)]
    public required string Name { get; init; }
}
