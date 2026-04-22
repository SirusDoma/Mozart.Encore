using Encore.Messaging;

namespace Memoryer.Messages.Events;

public class RankNotificationEventData : IMessage
{
    public static Enum Command => EventCommand.RankNotification;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public int MusicId { get; init; }

    [StringMessageField(order: 2)]
    public required string Nickname { get; init; }

    [MessageField(order: 4)]
    public int Ranking { get; init; }
}
