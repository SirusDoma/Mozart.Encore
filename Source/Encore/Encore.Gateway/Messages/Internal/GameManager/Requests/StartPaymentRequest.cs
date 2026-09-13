using Encore.Messaging;

namespace Encore.Messages.Requests;

public class StartPaymentRequest : IMessage
{
    public static Enum Command => ServerCommand.StartPayment;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }
}
