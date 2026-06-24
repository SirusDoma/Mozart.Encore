using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class MusicLoadedExResponse : IMessage
{
    public static Enum Command => ResponseCommand.MusicLoadedEx;
}
