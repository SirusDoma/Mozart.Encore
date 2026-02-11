using System.Net;

namespace Mozart.Data.Entities;

public interface ICredentialRequest
{
    IPAddress Address { get; init; }
}

public class UsernamePasswordCredentialRequest : ICredentialRequest
{
    public required string Username { get; init; }

    public required byte[] Password { get; init; }

    public required IPAddress Address { get; init; }
}
