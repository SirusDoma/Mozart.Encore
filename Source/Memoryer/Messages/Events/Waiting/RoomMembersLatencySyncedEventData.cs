using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Events;

public class RoomMembersLatencySyncedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomMembersLatencySynced;

    [MessageField(order: 0)]
    public GameSpeed ChampionSpeed { get; init; }

    [MessageField(order: 1)]
    public GameSpeed ChallengerSpeed { get; init; }
}
