using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class UserListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetUserList;

    public class UserInfo : SubMessage
    {
        [StringMessageField(order: 0)]
        public required string Nickname { get; init; }

        [MessageField(order: 1)]
        public int Level { get; init; }
    }

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<UserInfo> Users { get; init; }
}
