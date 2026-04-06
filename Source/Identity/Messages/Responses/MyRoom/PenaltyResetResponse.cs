using Encore.Messaging;

namespace Identity.Messages.Responses;

public class PenaltyResetResponse : IMessage
{
    public static Enum Command => ResponseCommand.PenaltyReset;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; }
}
