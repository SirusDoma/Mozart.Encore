using Encore.Messaging;

namespace Mozart;

public class PlayerLeaveGameEventData : IMessage
{
    public static Enum Command => EventCommand.PlayerLeaveGame;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public int Level { get; init; }
}