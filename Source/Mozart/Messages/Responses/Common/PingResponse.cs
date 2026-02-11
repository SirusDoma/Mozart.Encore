using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class PingResponse : IMessage
{
    public static Enum Command => GenericCommand.Ping;
}
