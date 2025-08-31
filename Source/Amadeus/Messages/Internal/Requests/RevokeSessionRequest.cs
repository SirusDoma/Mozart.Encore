using Encore.Messaging;

namespace Amadeus.Internal.Requests;

public class RevokeSessionRequest : IMessage
{
    public static Enum Command => GatewayCommand.RevokeSession;

    [StringMessageField(order: 0, maxLength: 128)]
    public required string SessionId { get; init; }
}