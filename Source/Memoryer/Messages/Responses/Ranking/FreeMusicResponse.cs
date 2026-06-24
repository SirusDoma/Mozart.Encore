using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class FreeMusicResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetFreeMusicStatus;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    private bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public bool FreeMusicEnabled { get; init; }
}
