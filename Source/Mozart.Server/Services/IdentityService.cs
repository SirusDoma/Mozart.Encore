using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Encore.Server;
using Microsoft.Extensions.Options;
using Mozart.Contexts;
using Mozart.Data.Entities;
using Mozart.Options;

namespace Mozart.Services;

public interface IIdentityService
{
    AuthOptions Options { get; }

    bool IsAuthCredentialSupported<TClientCredential>()
        where TClientCredential : class, ICredentialRequest;

    Task<string> Authenticate<TCredential>(TCredential request, CancellationToken cancellationToken)
        where TCredential : class, ICredentialRequest;

    Task<AuthSession> Authorize(string token, CancellationToken cancellationToken);

    Task UpdateChannel(string token, int serverId, int channelId, CancellationToken cancellationToken);

    Task Revoke(string token, CancellationToken cancellationToken);

    Task ClearSessions(CancellationToken cancellationToken);

    Task ClearSessions(int serverId, int channelId, CancellationToken cancellationToken);
}

public sealed class IdentityService(IAuthContext ctx, IOptions<ServerOptions> server, IOptions<TcpOptions> tcp,
    IOptions<GatewayOptions> gateway, IOptions<AuthOptions> options)
    : IIdentityService
{
    public AuthOptions Options => options.Value;

    public bool IsAuthCredentialSupported<TCredential>()
        where TCredential : class, ICredentialRequest
        => typeof(UsernamePasswordCredentialRequest).IsAssignableFrom(typeof(TCredential));

    public Task<string> Authenticate<TCredential>(TCredential credential, CancellationToken cancellationToken)
        where TCredential : class, ICredentialRequest
    {
        if (credential is not UsernamePasswordCredentialRequest request)
            throw new NotSupportedException($"{credential.GetType()} is not supported");

        return Authenticate(request, cancellationToken);
    }

    public async Task<string> Authenticate(UsernamePasswordCredentialRequest request,
        CancellationToken cancellationToken)
    {
        var record    = await ctx.FindCredential(request.Username, cancellationToken);
        bool verified = Options.Mode switch
        {
            AuthMode.Default => VerifyHashedPassword(record, request),
            AuthMode.Foreign => VerifyPlainPassword(record, request),
            _ => throw new UnreachableException()
        };

        if (!verified)
            throw new ArgumentException("Invalid username or password", nameof(request));

        string gatewayId = server.Value.Mode switch
        {
            DeploymentMode.Full    => tcp.Value.Address,
            DeploymentMode.Gateway => tcp.Value.Address,
            DeploymentMode.Channel => gateway.Value.Address,
            _ => tcp.Value.Address
        };

        var session = await ctx.CreateSession(gatewayId, record.Username, request.Address,
            cancellationToken);

        await ctx.Commit();
        return session.Token;
    }

    public async Task<AuthSession> Authorize(string token, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(token, out _))
        {
            if (Options.Mode == AuthMode.Default)
                throw new ArgumentException("Invalid token", nameof(token));

            string[] creds = token.Split(':');
            if (creds.Length != 2)
                throw new ArgumentException("Invalid token", nameof(token));

            token = await Authenticate(new UsernamePasswordCredentialRequest()
            {
                Username = creds[0],
                Password = Encoding.UTF8.GetBytes(creds[1]),
                Address  = IPAddress.Any
            }, cancellationToken);
        }

        var session = await ctx.Sessions.Find(token, cancellationToken);
        if (session == null)
            throw new ArgumentException("Invalid token", nameof(token));

        return session;
    }

    public async Task UpdateChannel(string token, int serverId, int channelId, CancellationToken cancellationToken)
    {
        await ctx.Sessions.UpdateChannel(token, serverId, channelId, cancellationToken);
    }

    public Task Revoke(string token, CancellationToken cancellationToken)
    {
        return ctx.Sessions.Revoke(token, cancellationToken);
    }

    public Task ClearSessions(CancellationToken cancellationToken)
    {
        return ctx.Sessions.Clear(cancellationToken);
    }

    public Task ClearSessions(int serverId, int channelId, CancellationToken cancellationToken)
    {
        return ctx.Sessions.Clear(serverId, channelId, cancellationToken);
    }

    private static bool VerifyHashedPassword(Credential record, UsernamePasswordCredentialRequest credentialRequest)
    {
        return PasswordHasher.Verify(record.Password, credentialRequest.Password);
    }

    private static bool VerifyPlainPassword(Credential record, UsernamePasswordCredentialRequest credentialRequest)
    {
        return Encoding.UTF8.GetString(record.Password).Trim().Equals(
            Encoding.UTF8.GetString(credentialRequest.Password).Trim());
    }
}

public static class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize  = 32; // 256 bits
    private const int Iterations = 10_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static byte[] Hash(byte[] password)
    {
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(salt);

        byte[] subkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        byte[] output = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt,   0, output, 0,        SaltSize);
        Buffer.BlockCopy(subkey, 0, output, SaltSize, KeySize);

        return output;
    }

    public static bool Verify(byte[] hashedPassword, byte[] password)
    {
        if (hashedPassword.Length != SaltSize + KeySize)
            return false;

        byte[] salt = new byte[SaltSize];
        Buffer.BlockCopy(hashedPassword, 0, salt, 0, SaltSize);

        byte[] storedSubkey = new byte[KeySize];
        Buffer.BlockCopy(hashedPassword, SaltSize, storedSubkey, 0, KeySize);

        byte[] generatedSubkey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return CryptographicOperations.FixedTimeEquals(storedSubkey, generatedSubkey);
    }
}
