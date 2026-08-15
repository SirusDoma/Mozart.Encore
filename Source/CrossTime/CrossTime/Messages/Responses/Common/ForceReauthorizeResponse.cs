using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class ForceReauthorizeResponse : IMessage
{
    public static Enum Command => ResponseCommand.ForceReauthorize;
}
