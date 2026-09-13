using Encore.Messaging;

namespace Encore.Messages.Requests;

public class MusicListRequest : IMessage
{
    public static Enum Command => ServerCommand.MusicList;

    [MessageField(order: 0)]
    public int UserId { get; init; }

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<int> MusicIds { get; init; } = [];
}
