using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class MusicMaxScoreRequest : IMessage
{
    public static Enum Command => RequestCommand.GetMusicMaxScore;

    [MessageField(order: 0)]
    public ushort MusicId { get; init; }

    [MessageField(order: 1)]
    public ushort NoteCount { get; init; }
}
