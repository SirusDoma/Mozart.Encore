using Encore.Messaging;

namespace Mozart;

public class PingResponse : IMessage
{
    public static Enum Command => GenericCommand.Ping;
}