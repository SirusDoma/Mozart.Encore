using Encore.Messaging;

namespace Encore.Messages.Requests;

public class AlbumScoreUpdateRequest : IMessage
{
    public static Enum Command => ServerCommand.AlbumScoreUpdate;

    [MessageField(order: 0)]
    public int UserId { get; init; }

    [MessageField(order: 1)]
    public int AlbumId { get; init; }

    [MessageField(order: 2)]
    public int Score { get; init; }
}
