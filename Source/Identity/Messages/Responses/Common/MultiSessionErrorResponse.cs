using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class MultiSessionErrorResponse : IMessage
{
    public static Enum Command => ResponseCommand.MultiSessionError;
}
