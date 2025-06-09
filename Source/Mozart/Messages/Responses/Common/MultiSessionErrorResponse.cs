using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class MultiSessionErrorResponse : IMessage
{
    public static Enum Command => ResponseCommand.MultiSessionError;
}