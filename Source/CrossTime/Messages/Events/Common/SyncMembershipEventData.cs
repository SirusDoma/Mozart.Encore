using Encore.Messaging;

namespace CrossTime.Messages.Events;

public class SyncMembershipEventData : IMessage
{
    public static Enum Command => EventCommand.SyncMembership;

    [MessageField(order: 0)]
    public int Gem { get; init; }

    [MessageField(order: 1)]
    public int Point { get; init; }

    [MessageField(order: 2)]
    public int O2Cash { get; init; }

    [MessageField(order: 3)]
    public int ItemCash { get; init; }

    [MessageField(order: 4)]
    public int MusicCash { get; init; }

    [MessageField(order: 5)]
    public int MembershipType { get; init; }
}
