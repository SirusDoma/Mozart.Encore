using Encore.Messaging;
using Mozart.Metadata.Items;

namespace Amadeus.Messages.Responses;

public class AcceptGiftResponse : IMessage
{
    public enum AcceptGiftResult : byte
    {
        Success      = 0x00000000, // 0
        NotDefined   = 0x00000001, // 1 or 2
        DbError      = 0x00000003, // 3
        UnknownError = 0x00000004, // 4
    }

    public static Enum Command => ResponseCommand.AcceptGift;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public AcceptGiftResult Result { get; init; }
}