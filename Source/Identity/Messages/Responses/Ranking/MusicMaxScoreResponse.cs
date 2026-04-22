using Encore.Messaging;

namespace Memoryer.Messages.Responses;

public class MusicMaxScoreResponse : IMessage
{
    public static Enum Command => ResponseCommand.GetMusicMaxScore;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    private bool Invalid { get; init; } = false;

    [MessageField(order: 1)]
    public uint MaxScore { get; init; }
}
