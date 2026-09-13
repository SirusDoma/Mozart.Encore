using Encore.Messaging;

namespace Encore.Messages.Responses;

public class SyncMusicPurchaseResponse : IMessage
{
    public static Enum Command => GameManagerCommand.SyncMusicPurchase;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int Gem { get; init; }

    [MessageField(order: 2)]
    public int Point { get; init; }

    [MessageField(order: 3)]
    public int O2Cash { get; init; }

    [CollectionMessageField(order: 4, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<int> MusicIds { get; init; } = [];

    [MessageField(order: 5)]
    public int MusicCash { get; init; }

    [MessageField(order: 6)]
    public int ItemCash { get; init; }
}
