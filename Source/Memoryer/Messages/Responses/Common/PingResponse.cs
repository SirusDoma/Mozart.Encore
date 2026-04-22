using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class PingResponse : IMessage
{
    public static Enum Command => GenericCommand.Ping;
}
