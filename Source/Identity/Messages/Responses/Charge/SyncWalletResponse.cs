using Encore.Messaging;

namespace Identity.Messages.Responses;

public class SyncCashPointResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncCashPoint;

    [MessageField(order: 0)]
    public int Gem { get; init; }

    [MessageField(order: 1)]
    public int CashPoint { get; init; }
}
