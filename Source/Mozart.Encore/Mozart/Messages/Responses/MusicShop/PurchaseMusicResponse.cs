using Encore.Messaging;

namespace Mozart;

public class PurchaseMusicResponse : IMessage
{
    public enum PurchaseResult : int
    {
        Success         = 0x00000000, // 0
        AlreadyUnlocked = 0x00000001, // 1
    }

    public static Enum Command => ResponseCommand.PurchaseMusic;

    [MessageField(order: 0)]
    public PurchaseResult Result { get; init; } = PurchaseResult.Success;
}