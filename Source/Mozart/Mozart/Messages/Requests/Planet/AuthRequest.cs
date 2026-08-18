using Encore.Messaging;
using Mozart.Messages.Codecs;

namespace Mozart.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    public abstract class AuthCredential
    {
        public abstract Version ClientVersion { get; }
        public required string Token    { get; init; }
    }

    public sealed class EGamesCredential : AuthCredential
    {
        public override Version ClientVersion => new(3, 00);
    }

    public sealed class GamaniaCredential : AuthCredential
    {
        public override Version ClientVersion => new(2, 93);
        public required string UserId { get; init; }
        public required string Unknown { get; init; }
    }

    [MessageField<AuthCredentialCodec>(order: 0)]
    public AuthCredential Credential { get; private set; } = null!;

    public Version ClientVersion => Credential.ClientVersion;
    public string  Token         => Credential.Token;
}
