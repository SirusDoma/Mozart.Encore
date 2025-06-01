using Encore.Messaging;

namespace Mozart;

public class MultiSessionErrorResponse : IMessage
{
    public static Enum Command => ResponseCommand.MultiSessionError;
}