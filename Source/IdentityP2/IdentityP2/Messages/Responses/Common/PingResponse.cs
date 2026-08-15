using Encore.Messaging;

namespace Identity.Messages.Responses;

public class PingResponse : IMessage
{
    public static Enum Command => GenericCommand.Ping;
}
