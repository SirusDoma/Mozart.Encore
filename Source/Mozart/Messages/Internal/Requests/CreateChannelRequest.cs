using Encore.Messaging;

namespace Mozart.Internal.Requests;

public class ChannelRelayRequest : IMessage
{
    public static Enum Command => ChannelCommand.Relay;

    [StringMessageField(order: 0, maxLength: 128)]
    public required string SessionId { get; init; }

    [CollectionMessageField(order: 1)]
    public required byte[] Payload { get; init; }
}
