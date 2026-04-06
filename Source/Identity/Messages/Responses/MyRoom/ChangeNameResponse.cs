using Encore.Messaging;

namespace Identity.Messages.Responses;

public class ChangeNameResponse : IMessage
{
    public static Enum Command => ResponseCommand.ChangeName;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; } = false;

    [StringMessageField(order: 1)]
    public required string Name { get; init; }
}
