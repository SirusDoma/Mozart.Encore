using Encore.Messaging;

namespace Encore.Messages.Requests;

public class SyncGemRequest : IMessage
{
    public static Enum Command => ServerCommand.SyncGem;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public IReadOnlyList<UserGem> Users { get; init; } = [];

    public class UserGem : SubMessage
    {
        [MessageField(order: 0)]
        public int UserId { get; init; }

        [MessageField(order: 1)]
        public int Gem { get; init; }
    }
}
