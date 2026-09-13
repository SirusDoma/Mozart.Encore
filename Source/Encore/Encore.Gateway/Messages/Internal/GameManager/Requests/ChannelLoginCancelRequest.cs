using Encore.Messaging;

namespace Encore.Messages.Requests;

public class ChannelLoginCancelRequest : IMessage
{
    public static Enum Command => ServerCommand.ChannelLoginCancel;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }

    [MessageField(order: 2)]
    public ushort ServerId { get; init; }

    [MessageField(order: 3)]
    public ushort ChannelId { get; init; }
}
