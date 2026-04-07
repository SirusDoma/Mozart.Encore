using System.Diagnostics.CodeAnalysis;
using Amadeus.Messages.Codecs;
using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public enum AuthResult : uint
{
    Success                 = 0x00000000,
    DbNetworkError          = 0x00000002, // 02
    NetworkError            = 0x00000003, // 03
    InsufficientBalance     = 0x0000000A, // 10 or 0x0000000B/11 or 0x00000021/33
    BillingError            = 0x0000000C, // 12
    MultiGamesSession       = 0x00000011, // 17
    IllegalUser             = 0x00000012, // 18
    BillingError20          = 0x00000014, // 20
    BillingError25          = 0x00000019, // 25
    BillingError26          = 0x0000001A, // 26
    BillingError27          = 0x0000001B, // 27
    BillingError30          = 0x0000001E, // 30
    BillingError31          = 0x0000001F, // 31
    BillingError35          = 0x00000023, // 35
    BillingError100         = 0x00000064, // 100
    BillingError200         = 0x000000C8, // 200
    BillingError201         = 0x000000C9, // 201
    BillingError210         = 0x000000D2, // 210
    BillingError211         = 0x000000D2, // 211
    DatabaseError           = 0xFFFFFF9B, // -101
    WebDbOpenError          = 0xFFFFFFF6, // -10
    WebDbQueryError         = 0xFFFFFFF7, // -9
    WebDbError              = 0xFFFFFFF8, // -8
    MemberTableQueryError   = 0xFFFFFFF9, // -7
    RegionError             = 0xFFFFFFFA, // -6
    Banned                  = 0xFFFFFFFB, // -5
    MultiGamesCertError     = 0xFFFFFFFC, // -4
    ServerDuplicateSessions = 0xFFFFFFFD, // -3
    ClientDuplicateSessions = 0xFFFFFFFE, // -2
    NetworkBillingError     = 0xFFFFFFFF, // -1
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
    LM,
    HB
}

public partial class AuthResponse : IMessage
{
    public static Enum Command => ResponseCommand.Authorize;

    public class SubscriptionInfo : SubMessage
    {
        [StringMessageField(order: 0, maxLength: 2, nullTerminated: false)]
        public BillingCode Billing { get; init; } = BillingCode.DB;

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
