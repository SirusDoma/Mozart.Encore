using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Requests;

public class SetRoomTitleRequest : IMessage
{
    public static Enum Command => RequestCommand.SetRoomTitle;

    [StringMessageField(order: 0, maxLength: 21)]
    public required string Title { get; init; }

    [MessageField(order: 1)]
    public required KeyMode KeyMode { get; init; }

    [MessageField(order: 2)]
    public required GameMode GameMode { get; init; }

    [MessageField(order: 3)]
    public bool HasPassword { get; init; }

    [StringMessageField(order: 4, maxLength: 12)]
    public required string Password { get; init; }

    [MessageField(order: 5)]
    public byte MinLevelLimit { get; set; } = 0;

    [MessageField(order: 6)]
    public byte MaxLevelLimit { get; set; } = 0;

    [MessageField(order: 7)]
    public int MusicId { get; set; } = 0;
}
