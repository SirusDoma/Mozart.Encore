using Encore.Messaging;

namespace Identity.Messages.Requests;

public class ConfirmMusicLoadedRequest : IMessage
{
    public static Enum Command => RequestCommand.ConfirmMusicLoaded;

    [MessageField(order: 0)]
    public int PowerSkillId { get; init; }
}
