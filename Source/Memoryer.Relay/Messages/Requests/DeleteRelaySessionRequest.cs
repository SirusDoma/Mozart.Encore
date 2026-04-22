using Encore.Messaging;

namespace Memoryer.Relay.Messages.Requests;

public class DeleteRelaySessionRequest : IMessage
{
    public static Enum Command => RelayCommand.DeleteSession;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Byte)]
    public required IReadOnlyList<RoomMember> Members { get; init; }

    public class RoomMember : SubMessage
    {
        [MessageField(order: 0)]
        public required int SessionKey1 { get; init; }

        [MessageField(order: 1)]
        public required int SessionKey2 { get; init; }
    }
}
