using Encore.Messaging;
using Memoryer.Messages.Events;

namespace Memoryer.Messages.Responses;

public class SyncMusicPurchaseResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncMusicPurchase;

    [MessageField(order: 0)]
    public int Gem { get; init; }

    [MessageField(order: 1)]
    public int Point { get; init; }

    [MessageField(order: 2)]
    public int O2Cash { get; init; }

    [MessageField(order: 3)]
    public int MusicCash { get; init; }

    [MessageField(order: 4)]
    public int ItemCash { get; init; }

    [MessageField(order: 5)]
    private int MusicExpiryCount => MusicList.Count;

    [MessageField(order: 6)]
    public int CashPoint { get; init; }

    [CollectionMessageField(order: 7)]
    public IReadOnlyList<MusicPremiumTimeEventData.MusicEntry> MusicList { get; init; } = [];

    [CollectionMessageField(order: 8, prefixSizeType: TypeCode.Int16)]
    public IList<CharacterInfoResponse.GiftItemInfo> ItemGiftBox { get; init; } = [];

    [CollectionMessageField(order: 9, prefixSizeType: TypeCode.Int16)]
    public IList<CharacterInfoResponse.GiftMusicInfo> MusicGiftBox { get; init; } = [];

    public TimeSpan FreePassExtensionPeriod { get; init; } = TimeSpan.Zero;

    [MessageField(order: 8)]
    private int FreePassExtensionDays => Math.Max((int)Math.Ceiling(FreePassExtensionPeriod.TotalDays), 0);
}
