using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class SetRoomSkillRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomSkill;

    [CollectionMessageField(order: 0, minCount: 1, maxCount: 3, prefixSizeType: TypeCode.Int32)]
    public IList<int> Skills { get; init; } = [];
}
