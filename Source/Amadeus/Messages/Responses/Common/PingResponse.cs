using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class PingResponse : IMessage
{
    public static Enum Command => GenericCommand.Ping;
}
