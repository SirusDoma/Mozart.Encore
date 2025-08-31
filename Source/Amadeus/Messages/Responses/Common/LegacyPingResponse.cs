using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class LegacyPingResponse : IMessage
{
    public static Enum Command => GenericCommand.LegacyPing;
}