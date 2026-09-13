using Encore.Data.Entities;
using Encore.Messaging;

namespace Encore.Messages.Responses;

public class SyncFreePassResponse : IMessage
{
    public static Enum Command => GameManagerCommand.SyncFreePass;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int Gem { get; init; }

    [MessageField(order: 2)]
    public int Point { get; init; }

    [MessageField(order: 3)]
    public int O2Cash { get; init; }

    [MessageField(order: 4)]
    public int MusicCash { get; init; }

    [MessageField(order: 5)]
    public int ItemCash { get; init; }

    [MessageField(order: 6)]
    public FreePassType FreePassType { get; init; }

    [StringMessageField(order: 7)]
    public string FreePassDuration { get; init; } = string.Empty;

    [MessageField(order: 8)]
    public int FreePassExpiry { get; init; }
}
