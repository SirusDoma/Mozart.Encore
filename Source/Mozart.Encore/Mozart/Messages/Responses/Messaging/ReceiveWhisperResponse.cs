using Encore.Messaging;

namespace Mozart;

public class ReceiveWhisperResponse : IMessage
{
    public static Enum Command => ResponseCommand.ReceiveWhisper;

    [StringMessageField(order: 0)]
    public required string Sender { get; init; }

    [StringMessageField(order: 1)]
    public string Content { get; init; } = string.Empty;
}
