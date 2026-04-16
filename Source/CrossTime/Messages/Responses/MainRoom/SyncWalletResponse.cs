using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class SyncWalletResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncWallet;

    [MessageField(order: 0)]
    public int Gem { get; init; }

    [MessageField(order: 1)]
    public int GemStar { get; init; }

    [MessageField(order: 2)]
    public int Point { get; init; }
}
