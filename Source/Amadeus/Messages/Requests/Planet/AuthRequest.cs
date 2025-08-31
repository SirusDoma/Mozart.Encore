using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    [StringMessageField(order: 0)]
    public string Token { get; private set; } = string.Empty;

    [StringMessageField(order: 1)]
    public string ClientId { get; private set; } = string.Empty;
}