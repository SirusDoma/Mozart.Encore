using Encore.Messaging;

namespace Amadeus.Internal.Requests;

public class GetChannelStatsRequest : IMessage
{
    public static Enum Command => GatewayCommand.GetChannelStats;

    [StringMessageField(order: 0, maxLength: 128)]
    public string RequestId { get; init; } = string.Empty;
}