using Encore.Messaging;

namespace Identity.Messages.Events;

public class UserLeaveWaitingEventData : IMessage
{
    public static Enum Command => EventCommand.UserLeaveWaiting;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public byte NewRoomMasterMemberId { get; init; }

    [MessageField<MessageFieldCodec<short>>(order: 2)]
    public bool Premium { get; init; }
}
