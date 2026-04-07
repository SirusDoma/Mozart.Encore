using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class PremiumAuthRequest : IMessage
{
    public static Enum Command => RequestCommand.ConnectGateway;
}
