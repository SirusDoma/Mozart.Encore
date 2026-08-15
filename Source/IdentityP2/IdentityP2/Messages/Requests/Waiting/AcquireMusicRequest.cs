using Encore.Messaging;

namespace Identity.Messages.Requests;

public class AcquireMusicRequest : IMessage
{
    public static Enum Command => RequestCommand.AcquireMusicRequest;

    [CollectionMessageField(order: 0, prefixSizeType: TypeCode.Byte)]
    public required IReadOnlyList<byte> MemberIds { get; init; }
}
