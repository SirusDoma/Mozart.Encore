using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class MultiSessionErrorResponse : IMessage
{
    public static Enum Command => ResponseCommand.MultiSessionError;
}
