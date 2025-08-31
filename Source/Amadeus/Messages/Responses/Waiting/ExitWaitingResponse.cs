using Encore.Messaging;

namespace Amadeus.Messages.Responses;

public class ExitWaitingResponse : IMessage
{
    public static Enum Command => ResponseCommand.ExitWaiting;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Failed { get; init; } = false;
}