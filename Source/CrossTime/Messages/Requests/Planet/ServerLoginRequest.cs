using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class ServerLoginRequest : IMessage
{
    public static Enum Command => RequestCommand.ServerLogin;

    [StringMessageField(order: 0)]
    public string Token { get; private set; } = string.Empty;

    [MessageField(order: 1)]
    public int ServerId { get; private set; }
}
