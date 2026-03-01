using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class SyncGemResponse : IMessage
{
    public static Enum Command => ResponseCommand.SyncGem;

    [MessageField(order: 0)]
    public int Gem { get; init; }
}
