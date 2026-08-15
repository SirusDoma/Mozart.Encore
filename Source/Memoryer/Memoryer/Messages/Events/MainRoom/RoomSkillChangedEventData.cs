using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class RoomSkillChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomSkillChanged;

    [MessageField(order: 0)]
    public int Number { get; init; }

    [CollectionMessageField(order: 1, prefixSizeType: TypeCode.Int32)]
    public IList<int> Skills { get; init; } = [];
}
