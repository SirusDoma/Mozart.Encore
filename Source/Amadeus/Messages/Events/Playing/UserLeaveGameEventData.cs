using Encore.Messaging;
using Mozart.Data.Entities;

namespace Amadeus.Messages.Events;

public class UserLeaveGameEventData : IMessage
{
    public static Enum Command => EventCommand.UserLeaveGame;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public int Level { get; init; }

    [MessageField(order: 2)]
    public int CashPoint { get; init; }

    [MessageField(order: 3)]
    private FreePassType FreePassType => FreePass.Type;

    public FreePass FreePass { get; init; } = new FreePass(FreePassType.None, DateTime.MinValue);
}
