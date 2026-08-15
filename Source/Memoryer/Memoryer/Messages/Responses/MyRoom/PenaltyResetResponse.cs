using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class PenaltyResetResponse : IMessage
{
    public static Enum Command => ResponseCommand.PenaltyReset;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; }
}
