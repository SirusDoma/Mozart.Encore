using Encore.Messaging;

namespace Mozart.Internal.Requests;

public class GrantSessionResponse : IMessage
{
    public static Enum Command => ChannelCommand.GrantSession;

    [MessageField(order: 0)]
    public required bool Success { get; init; }

    [StringMessageField(order: 1, maxLength: 128)]
    public required string SessionId { get; init; }

    [MessageField(order: 2)]
    public required int ChannelId { get; init; }

    [MessageField(order: 3)]
    public required int Capacity { get; init; }

    [MessageField(order: 4)]
    public required int UserCount { get; init; }
}
