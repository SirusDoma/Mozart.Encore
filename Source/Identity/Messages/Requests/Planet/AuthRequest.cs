using Identity.Messages.Codecs;
using Encore.Messaging;
using Mozart.Metadata;

namespace Identity.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    [MessageField(order: 0)]
    public int UserId { get; private set; }

    [StringMessageField(order: 1)]
    public string Token { get; private set; } = string.Empty;

    [MessageField<AuthGenderCodec>(order: 1)]
    public Gender Gender { get; private set; } = Gender.Any;

    [MessageField(order: 2)]
    private byte Unused { get; set; } // Hardcoded to 1

    [StringMessageField(order: 3)]
    public string ClientId { get; private set; } = string.Empty;
}
