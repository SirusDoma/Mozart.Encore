using Encore.Messaging;

namespace Encore.Messages.Responses;

public class ClaimGiftResponse : IMessage
{
    public static Enum Command => GameManagerCommand.ClaimGift;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int Result { get; init; }
}
