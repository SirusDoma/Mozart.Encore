using Encore.Messaging;

namespace Mozart;

public class PlayerLeaveWaitingEventData : IMessage
{
    public static Enum Command => EventCommand.PlayerLeaveWaiting;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public byte NewRoomMasterMemberId { get; init; }
}