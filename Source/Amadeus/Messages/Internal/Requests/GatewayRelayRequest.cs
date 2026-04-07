using Encore.Messaging;

namespace Amadeus.Internal.Requests;

public class GatewayRelayRequest : IMessage
{
    public static Enum Command => GatewayCommand.Relay;

    [StringMessageField(order: 0, maxLength: 128)]
    public required string SessionId { get; init; }

    [CollectionMessageField(order: 1)]
    public required byte[] Payload { get; init; }
}
