using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class UpdateMusicStateRequest : IMessage
{
    public static Enum Command => RequestCommand.UpdateMusicState;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}
