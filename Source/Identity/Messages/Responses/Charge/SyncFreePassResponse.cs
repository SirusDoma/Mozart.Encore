using Encore.Messaging;
using Memoryer.Messages.Codecs;
using Mozart.Data.Entities;

namespace Memoryer.Messages.Responses;

public class SyncFreePassResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncFreePass;

    public enum SyncResult : uint
    {
        Success = 0x00000000, // 0
        Unknown = 0xFFFFFFA2  // -94
    }

    public class UserFreePassInfo : SubMessage
    {
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
        public FreePassType FreePassType { get; init; } = FreePassType.None;

        [MessageField(order: 6)]
        public int CashPoint { get; init; }

        [StringMessageField(order: 7)]
        private string FreePassFormattedDuration =>
            FreePassExpiry == TimeSpan.Zero ? string.Empty : FreePassExpiry.ToString();

        [MessageField<SubscriptionTimeExpiryCodec>(order: 8)]
        public TimeSpan FreePassExpiry { get; init; } = TimeSpan.Zero;
    }

    [MessageField(order: 0)]
    public SyncResult Result { get; init; }

    [MessageField(order: 1)]
    public UserFreePassInfo? Info { get; init; } = null;
}
