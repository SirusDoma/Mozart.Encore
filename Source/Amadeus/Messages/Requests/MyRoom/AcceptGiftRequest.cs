using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Requests;

public class AcceptGiftRequest : IMessage
{
    public static Enum Command => RequestCommand.AcceptGift;

    [MessageField(order: 0)]
    public GiftType GiftType { get; init; }

    [MessageField(order: 1)]
    public int GiftId { get; init; }

    [MessageField(order: 2)]
    public int ItemId { get; init; }

    [MessageField(order: 3)]
    public bool IsAttributiveItem { get; init; }
}