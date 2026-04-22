using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class ForceReauthorizeResponse : IMessage
{
    public static Enum Command => ResponseCommand.ForceReauthorize;
}
