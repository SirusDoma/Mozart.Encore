using Encore.Messaging;

namespace Identity.Messages.Responses;

public class CheckGiftFreePassResponse : IMessage
{
    public static Enum Command => ResponseCommand.CheckGiftFreePass;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; }

    [StringMessageField(order: 1)]
    public required string Username { get; init; }
}
