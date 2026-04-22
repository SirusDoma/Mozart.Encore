using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class AcquireMusicRequest : IMessage
{
    public static Enum Command => RequestCommand.AcquireMusicRequest;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Byte)]
    public required IReadOnlyList<byte> MemberIds { get; init; }
}
