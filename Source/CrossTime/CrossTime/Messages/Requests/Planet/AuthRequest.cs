using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    [StringMessageField(order: 0)]
    public required string Token { get; init; }
}
