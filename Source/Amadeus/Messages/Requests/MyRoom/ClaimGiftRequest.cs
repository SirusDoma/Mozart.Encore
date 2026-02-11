using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Requests;

public class ClaimGiftRequest : IMessage
{
    public static Enum Command => RequestCommand.ClaimGift;

    [MessageField(order: 0)]
    public GiftType GiftType { get; init; }

    [MessageField(order: 1)]
    public int GiftId { get; init; }

    [MessageField(order: 2)]
    public int ResourceId { get; init; }

    [MessageField(order: 3)]
    public bool IsAttributiveItem { get; init; }
}