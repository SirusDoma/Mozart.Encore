using Encore.Messaging;

namespace Amadeus.Internal.Requests;

public class GrantSessionRequest : IMessage
{
    public static Enum Command => GatewayCommand.GrantSession;

    [StringMessageField(order: 0, maxLength: 128)]
    public required string SessionId { get; init; }

    [StringMessageField(order: 1, maxLength: 128)]
    public required string ClientId { get; init; }
}