using Encore.Messaging;
using Mozart.Messages.Codecs;

namespace Mozart.Messages.Responses;

public class SubscriptionAlertResponse : IMessage
{
    public static Enum Command => ResponseCommand.SubscriptionAlert;

    public enum AlertType : uint
    {
        SubscriptionExpired = 0x00000000,
        DayBlockExpiring    = 0x00000001,
        None                = 0xFFFFFFFD
    }

    [MessageField(order: 0)]
    public required AlertType Type { get; init; }

    [StringMessageField(order: 1, maxLength: 2, nullTerminated: false)]
    public required BillingCode Billing { get; init; }

    [MessageField<AuthTimestampCodec>(order: 2)]
    public DateTime CurrentTimestamp { get; init; } = DateTime.Now;

    [MessageField<TimeSpanMinuteCodec>(order: 3)]
    public TimeSpan SubscriptionRemainingTime { get; init; } = TimeSpan.Zero;
}
