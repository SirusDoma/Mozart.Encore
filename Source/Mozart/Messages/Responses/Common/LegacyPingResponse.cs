using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class LegacyPingResponse : IMessage
{
    public static Enum Command => GenericCommand.LegacyPing;
}
