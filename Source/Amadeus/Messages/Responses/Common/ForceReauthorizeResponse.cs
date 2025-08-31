using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class ForceReauthorizeResponse : IMessage
{
    public static Enum Command => ResponseCommand.ForceReauthorize;
}