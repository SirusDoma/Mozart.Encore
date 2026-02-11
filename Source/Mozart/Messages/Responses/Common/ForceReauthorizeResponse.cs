using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class ForceReauthorizeResponse : IMessage
{
    public static Enum Command => ResponseCommand.ForceReauthorize;
}
