using Encore.Messaging;

namespace Amadeus.Internal.Requests;

public class GetChannelStatsResponse : IMessage
{
    public static Enum Command => ChannelCommand.GetChannelStats;

    [StringMessageField(order: 0, maxLength: 128)]
    public required string RequestId { get; init; }

    [MessageField(order: 1)]
    public required int Id { get; init; }

    [MessageField(order: 2)]
    public required int Capacity { get; init; }

    [MessageField(order: 3)]
    public required int UserCount { get; init; }

    [MessageField(order: 4)]
    public float Gem { get; init; } = 1.0f;

    [MessageField(order: 5)]
    public float Exp { get; init; } = 1.0f;
}