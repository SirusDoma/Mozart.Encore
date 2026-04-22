using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class WaitingSkillChangedEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingSkillChanged;

    [CollectionMessageField(order: 0, minCount: 0, maxCount: 3, prefixSizeType: TypeCode.Int32)]
    public IList<int> Skills { get; init; } = [];

    [MessageField<MessageFieldCodec<int>>(order: 1)]
    public bool HasSuperRoomManager { get; init; }

    [MessageField(order: 2)]
    public byte SuperRoomManagerMemberId { get; init; }
}
