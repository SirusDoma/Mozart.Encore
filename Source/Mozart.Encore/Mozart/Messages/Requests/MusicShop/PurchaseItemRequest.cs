using Encore.Messaging;

namespace Mozart;

public class PurchaseMusicRequest : IMessage
{
    public static Enum Command => RequestCommand.PurchaseMusic;

    [MessageField(order: 0)]
    public int MusicId { get; init; }
}