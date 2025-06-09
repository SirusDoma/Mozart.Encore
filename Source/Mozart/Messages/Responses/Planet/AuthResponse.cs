using System.Diagnostics.CodeAnalysis;
using Encore.Messaging;
using Mozart.Messages.Codecs;

namespace Mozart.Messages.Responses;

public enum AuthResult : uint
{
    Success            = 0x00000000,
    NetworkError       = 0x00000003, // 03
    InsufficientBalnce = 0x0000000A, // 10 or 0x0000000B/11 or 0x00000021/33
    MultiGamesSession  = 0x00000011, // 17
    IllegalUser        = 0x00000012, // 18
    DatabaseError      = 0xFFFFFF9B, // -101
    Banned             = 0xFFFFFFFB, // -5
    DuplicateSessions  = 0xFFFFFFFE, // -2,
    InvalidCredentials = 0xFFFFFFFF, // -1,
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum BillingCode
{
    FM,
    DB, // Day-Block
    FD,
    LE,
    TH,
    TB, // Time-Block
    LD,
    LM
}

public partial class AuthResponse : IMessage
{
    public static Enum Command => ResponseCommand.Authorize;

    public class SubscriptionInfo : SubMessage
    {
        [StringMessageField(order: 0, maxLength: 2, nullTerminated: false)]
        public BillingCode Billing { get; init; } = BillingCode.TB;

        [MessageField<AuthTimestampCodec>(order: 1)]
        public DateTime CurrentTimestamp { get; init; } = DateTime.Now;

        [MessageField<TimeSpanMinuteCodec>(order: 2)]
        public TimeSpan SubscriptionRemainingTime { get; init; } = TimeSpan.Zero;
    }

    [MessageField(order: 0)]
    public AuthResult Result { get; init; }

    [MessageField(order: 1)]
    public required SubscriptionInfo Subscription { get; init; }
}