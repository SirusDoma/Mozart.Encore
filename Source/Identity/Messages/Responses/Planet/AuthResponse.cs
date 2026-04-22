using Encore.Messaging;
using Memoryer.Messages.Codecs;
using Mozart.Data.Entities;

namespace Memoryer.Messages.Responses;

public enum AuthResult : uint
{
    Success          = 0x00000000,
    InvalidLogin     = 0xFFFFFFFF, // -1
    AlreadyConnected = 0xFFFFFFFE, // -2
    RegionError      = 0xFFFFFFFD, // -3
    UnauthorizedPc   = 0xFFFFFFFC, // -4
    Banned           = 0xFFFFFFFB, // -5
    TimeExceeded     = 0xFFFFFFFA, // -6
    DatabaseError    = 0xFFFFFF9B, // -101
}

public class AuthResponse : IMessage
{
    public static Enum Command => ResponseCommand.Authorize;

    public class FreePassInfo : SubMessage
    {
        [MessageField(order: 0)]
        public FreePassType Type { get; init; } = FreePassType.None;

        [StringMessageField(order: 1)]
        private string FormattedDuration => Expiry == TimeSpan.Zero ? string.Empty : Expiry.ToString();

        [MessageField<SubscriptionTimeExpiryCodec>(order: 2)]
        public TimeSpan Expiry { get; init; } = TimeSpan.Zero;
    }

    public class StarterPassInfo : SubMessage
    {
        [MessageField<MessageFieldCodec<short>>(order: 0)]
        public bool Active { get; init; }

        public TimeSpan Expiry { get; init; } = TimeSpan.Zero;

        [StringMessageField(order: 2)]
        private string? FormattedDuration => !Active || Expiry == TimeSpan.Zero ? null : Expiry.ToString();
    }

    public class InfiniteRingInfo : SubMessage
    {
        [MessageField<MessageFieldCodec<short>>(order: 0)]
        public bool Active { get; init; }

        public TimeSpan Expiry { get; init; } = TimeSpan.Zero;

        [StringMessageField(order: 2)]
        private string? FormattedDuration => !Active || Expiry == TimeSpan.Zero ? null : Expiry.ToString();
    }

    [MessageField(order: 0)]
    public AuthResult Result { get; init; }

    [MessageField(order: 1)]
    public FreePassInfo? FreePass { get; init; }

    [MessageField(order: 2)]
    public StarterPassInfo? StarterPass { get; init; }

    [MessageField(order: 3)]
    public InfiniteRingInfo? InfiniteRing { get; init; }
}
