using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class SetRoomSkillExRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomSkillEx;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Int32)]
    public required IReadOnlyList<int> Skills { get; init; }
}
