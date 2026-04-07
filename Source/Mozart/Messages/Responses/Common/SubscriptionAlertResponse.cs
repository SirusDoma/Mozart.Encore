using System.Text;
using Encore.Messaging;

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

    public required BillingCode Billing { get; init; }

    [MessageField(order: 0)]
    public required AlertType Type { get; init; }

    [MessageField(order: 1)]
    private byte[] Payload => Type switch
    {
        AlertType.SubscriptionExpired => Encoding.UTF8.GetBytes(Enum.GetName(Billing)!),
        _                             => new byte[12] // padding
    };
}
