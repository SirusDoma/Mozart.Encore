using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class SyncMusicPurchaseResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncMusicPurchase;

    [MessageField(order: 0)]
    public int Gem { get; init; }

    [MessageField(order: 1)]
    public int Point { get; init; }

    [MessageField(order: 2)]
    public int O2Cash { get; init; }

    [CollectionMessageField(order: 3, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<ushort> MusicIds { get; init; } = [];

    [MessageField(order: 4)]
    public int ItemCash { get; init; }

    [MessageField(order: 5)]
    public int MusicCash { get; init; }
}