using Encore.Messaging;
using Encore.Metadata;
using Identity.Messages.Codecs;

namespace Identity.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    [MessageField(order: 0)]
    public int UserId { get; private set; }

    [StringMessageField(order: 1)]
    public string Token { get; private set; } = string.Empty;

    [MessageField<AuthGenderCodec>(order: 2)]
    public Gender Gender { get; private set; } = Gender.Any;

    [MessageField(order: 3)]
    private byte Unused { get; set; } // Hardcoded to 1

    [StringMessageField(order: 4)]
    public string ClientId { get; private set; } = string.Empty;
}
