using Encore.Messaging;

namespace Mozart.Messages.Requests;

public class MusicListRequest : IMessage
{
    public static Enum Command => RequestCommand.SendMusicList;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<int> MusicIds { get; init; }
}