using Encore.Messaging;

namespace Mozart.Internal.Requests;

public class GrantSessionRequest : IMessage
{
    public static Enum Command => GatewayCommand.GrantSession;

    [StringMessageField(order: 0, maxLength: 128)]
    public required string SessionId { get; init; }
}