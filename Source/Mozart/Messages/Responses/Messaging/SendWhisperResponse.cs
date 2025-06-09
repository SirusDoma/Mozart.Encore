using Encore.Messaging;

namespace Mozart.Messages.Responses;

public class SendWhisperResponse : IMessage
{
    public static Enum Command => ResponseCommand.SendWhisper;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = true;

    [StringMessageField(order: 1)]
    public required string Sender { get; init; }

    [StringMessageField(order: 2)]
    public string Content { get; init; } = string.Empty;
}
