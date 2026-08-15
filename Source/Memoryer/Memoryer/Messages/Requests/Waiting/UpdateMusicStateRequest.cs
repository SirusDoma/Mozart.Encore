using Encore.Messaging;
using Encore.Metadata;

namespace Memoryer.Messages.Requests;

public class UpdateMusicStateRequest : IMessage
{
    public static Enum Command => RequestCommand.UpdateMusicState;

    [MessageField(order: 0)]
    public MusicState State { get; init; }
}
