using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class MultiSessionErrorResponse : IMessage
{
    public static Enum Command => ResponseCommand.MultiSessionError;
}