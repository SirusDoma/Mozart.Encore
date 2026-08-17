using Amadeus.Messages.Codecs;
using Encore.Messaging;

namespace Amadeus.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    public abstract class AuthCredential
    {
        public abstract Version ClientVersion { get; }
        public required string Token    { get; init; }
        public required string ClientId { get; init; }
    }

    public sealed class EGamesCredential : AuthCredential
    {
        public override Version ClientVersion => new(3, 82);
    }

    public sealed class GamaniaCredential : AuthCredential
    {
        public const int TokenSplitLength = 18;

        public override Version ClientVersion => new(3, 0);
        public required string UserId { get; init; }
        public bool Unknown { get; init; }
    }


    [MessageField<AuthCredentialCodec>(order: 0)]
    public AuthCredential Credential { get; private set; } = null!;

    public Version ClientVersion => Credential.ClientVersion;
    public string  Token         => Credential.Token;
    public string  ClientId      => Credential.ClientId;
}
