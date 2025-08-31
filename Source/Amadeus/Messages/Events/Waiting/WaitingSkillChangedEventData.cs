using Encore.Messaging;

namespace Amadeus.Messages.Events.Waiting;

public class WaitingSkillChangedEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingSkillChanged;

    [CollectionMessageField(order: 0, minCount: 0, maxCount: 3, prefixSizeType: TypeCode.Int32)]
    public IList<int> Skills { get; init; } = [];
}