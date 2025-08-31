using Encore.Messaging;

namespace Amadeus.Messages.Responses;

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

    [MessageField(order: 1)]
    public int Unknown1 { get; init; }

    [MessageField(order: 2)]
    public int Unknown2 { get; init; }

    [CollectionMessageField(order: 3)]
    public IReadOnlyList<ushort> Unknown3 { get; init; } = []; // Music ids?

    [MessageField(order: 4)]
    public int Unknown4 { get; init; }

    [MessageField(order: 5)]
    public int Unknown5 { get; init; }
}