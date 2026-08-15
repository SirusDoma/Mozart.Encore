using Encore.Messaging;

namespace Identity.Messages.Responses;

public class CheckNameResponse : IMessage
{
    public static Enum Command => ResponseCommand.CheckName;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Exists { get; init; }

    [StringMessageField(order: 1)]
    public required string Name { get; init; }
}
