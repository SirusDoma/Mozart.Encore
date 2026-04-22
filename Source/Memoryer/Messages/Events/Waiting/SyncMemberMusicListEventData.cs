using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class SyncMemberMusicListEventData : IMessage
{
    public static Enum Command => EventCommand.SyncMemberMusicList;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<ushort> MusicIds { get; init; } = [];
}
