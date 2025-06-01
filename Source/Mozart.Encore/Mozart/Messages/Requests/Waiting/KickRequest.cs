using Encore.Messaging;

namespace Mozart;

public class KickRequest : IMessage
{
    public static Enum Command => RequestCommand.Kick;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}