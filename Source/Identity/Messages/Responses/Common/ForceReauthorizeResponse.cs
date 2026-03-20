using Encore.Messaging;

namespace Identity.Messages.Responses;

public class ForceReauthorizeResponse : IMessage
{
    public static Enum Command => ResponseCommand.ForceReauthorize;
}
