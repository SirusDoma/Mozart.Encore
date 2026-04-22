using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class SyncMemberMusicStateEventData : IMessage
{
    public static Enum Command => EventCommand.SyncMemberMusicState;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public MusicState State { get; init; }
}
