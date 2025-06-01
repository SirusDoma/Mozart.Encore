using Encore.Messaging;

namespace Mozart;

public class LegacyPingResponse : IMessage
{
    public static Enum Command => GenericCommand.LegacyPing;
}