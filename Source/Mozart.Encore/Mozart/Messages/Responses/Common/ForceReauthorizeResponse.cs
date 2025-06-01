using Encore.Messaging;

namespace Mozart;

public class ForceReauthorizeResponse : IMessage
{
    public static Enum Command => ResponseCommand.ForceReauthorize;
}