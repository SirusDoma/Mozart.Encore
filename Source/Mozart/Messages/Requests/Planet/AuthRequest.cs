using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    [StringMessageField(order: 0)]
    public string Token { get; private set; } = string.Empty;
}