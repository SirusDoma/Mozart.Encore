using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class UserListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetUserList;

    public class UserInfo : SubMessage
    {
        [MessageField(order: 0)]
        public int UserIndexId { get; init; }

        [StringMessageField(order: 1)]
        public required string Username { get; init; }

        [StringMessageField(order: 2)]
        public required string Nickname { get; init; }

        [MessageField(order: 3)]
        public int Level { get; init; }
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<UserInfo> Users { get; init; }
}
