using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class CheckGiftFreePassRequest : IMessage
{
    public static Enum Command =>  RequestCommand.CheckGiftFreePass;

    [StringMessageField(order: 0)]
    public required string Name { get; init; }
}
