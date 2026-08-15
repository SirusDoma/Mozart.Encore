using Encore.Messaging;

namespace Amadeus.Internal.Requests;

public class GrantSessionResponse : IMessage
{
    public static Enum Command => ChannelCommand.GrantSession;

    [MessageField(order: 0)]
    public required bool Success { get; init; }

    [StringMessageField(order: 1, maxLength: 128)]
    public required string SessionId { get; init; }

    [StringMessageField(order: 2, maxLength: 128)]
    public required string ClientId { get; init; }

    [StringMessageField(order: 3, maxLength: 128)]
    public required string Username { get; init; }

    [StringMessageField(order: 4, maxLength: 128)]
    public required string Nickname { get; init; }

    [MessageField(order: 5)]
    public required int Ranking { get; init; }

    [MessageField(order: 6)]
    public required int ChannelId { get; init; }

    [MessageField(order: 7)]
    public required int Capacity { get; init; }

    [MessageField(order: 8)]
    public required int UserCount { get; init; }
}
