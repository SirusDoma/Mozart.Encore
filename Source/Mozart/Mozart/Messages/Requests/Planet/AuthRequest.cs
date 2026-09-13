using Encore.Messaging;
using Mozart.Messages.Codecs;

namespace Mozart.Messages.Requests;

public class AuthRequest : IMessage
{
    public static Enum Command => RequestCommand.Authorize;

    public abstract class AuthCredential
    {
        public abstract Version ClientVersion { get; }
        public abstract string ClientId { get; }
        public required string Token    { get; init; }
    }

    public sealed class EGamesCredential : AuthCredential
    {
        // Arbitrary Client ID for 3.10; not present in the original client.
        public const string Id = "ARR1QG-YNLY-7XCC1Y-389S-CLEUFO";

        public override Version ClientVersion => new(3, 10);
        public override string ClientId => Id;
    }

    public sealed class GamaniaCredential : AuthCredential
    {
        // Arbitrary Client ID for 2.93; not present in the original client.
        public const string Id = "8WFMWG-CRVH-DSATYQ-EOBH-AGVG3M";

        public override Version ClientVersion => new(2, 93);
        public override string ClientId => Id;
        public required string UserId { get; init; }
        public required string Unknown { get; init; }
    }

    [MessageField<AuthCredentialCodec>(order: 0)]
    public AuthCredential Credential { get; private set; } = null!;

    public Version ClientVersion => Credential.ClientVersion;
    public string  ClientId      => Credential.ClientId;
    public string  Token         => Credential.Token;
}
