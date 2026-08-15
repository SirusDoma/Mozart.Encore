using Encore.Messaging;

namespace Identity.Messages.Responses;

public class MultiSessionErrorResponse : IMessage
{
    public static Enum Command => ResponseCommand.MultiSessionError;
}
