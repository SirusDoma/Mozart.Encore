using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Requests;

public class ConfirmMusicLoadedRequest : IMessage
{
    public static Enum Command => RequestCommand.ConfirmMusicLoaded;

    [MessageField(order: 0)]
    public int PowerSkillId { get; init; }

    [MessageField(order: 1)]
    public GameSpeed Speed { get; init; }
}
