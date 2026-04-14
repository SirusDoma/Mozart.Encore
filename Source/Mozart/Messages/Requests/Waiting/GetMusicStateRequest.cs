using Encore.Messaging;

namespace Identity.Messages.Events;

public class GetMusicStateRequest : IMessage
{
    public static Enum Command => RequestCommand.GetMusicState;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}
