using Encore.Messaging;

namespace Encore.Messages.Responses;

public class StartPaymentResponse : IMessage
{
    public static Enum Command => GameManagerCommand.StartPayment;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }
}
