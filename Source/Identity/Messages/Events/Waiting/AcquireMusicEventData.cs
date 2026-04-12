using Encore.Messaging;
using Mozart.Metadata;

namespace Identity.Messages.Events;

public class AcquireMusicEventData : IMessage
{
    public static Enum Command => EventCommand.AcquireMusic;

    public class MemberMusicState : SubMessage
    {
        [MessageField(order: 0)]
        public byte MemberId { get; init; }

        [MessageField(order: 1)]
        public MusicState State { get; init; }
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Byte)]
    public required IReadOnlyList<MemberMusicState> States { get; init; }
}
