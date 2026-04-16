using Encore.Messaging;

namespace Identity.Messages.Requests;

public class SyncMemberMusicStateRequest : IMessage
{
    public static Enum Command => RequestCommand.SyncMemberMusicState;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}
