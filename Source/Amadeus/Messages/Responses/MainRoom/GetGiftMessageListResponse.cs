using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Responses;

public class GetGiftMessageListResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetGiftMessageList;

    public class GiftEntry : SubMessage
    {
        [MessageField(order: 0)]
        public int MessageId { get; init; }

        [MessageField(order: 1)]
        public GiftType GiftType { get; init; }

        [StringMessageField(order: 2)]
        public required string WriteDate { get; init; }

        [StringMessageField(order: 3)]
        public required string Sender { get; init; }

        [StringMessageField(order: 4)]
        public required string Title { get; init; }

        [StringMessageField(order: 5)]
        public required string Content { get; init; }
    }

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [CollectionMessageField(order:1, prefixSizeType: TypeCode.Int16)]
    public IReadOnlyList<GiftEntry> Messages { get; init; } = [];
}
