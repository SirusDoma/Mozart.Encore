using System.Net;
using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;

namespace Mozart.Contexts;

public interface IAuthContext : IContext
{
    ICredentialRepository Credentials { get; }
    IUserRepository       Users       { get; }
    ISessionRepository    Sessions    { get; }

    Task<Credential> FindCredential(string username, CancellationToken cancellationToken = default);

    Task<AuthSession> CreateSession(string gatewayId, string username,  IPAddress clientAddress,
        CancellationToken cancellationToken = default);
}

public class AuthContext : IAuthContext
{
    private readonly MainDbContext _context;

    public AuthContext(IDbContextFactory<MainDbContext> factory)
    {
        var staticFactory = new StaticDbContextFactory<MainDbContext>(factory);

        _context     = staticFactory.CreateDbContext();
        Credentials  = new CredentialRepository(staticFactory);
        Users        = new UserRepository(staticFactory);
        Sessions     = new SessionRepository(staticFactory);
    }

    public ICredentialRepository Credentials { get; }
    public IUserRepository       Users       { get; }
    public ISessionRepository    Sessions    { get; }

    public async Task<Credential> FindCredential(string username, CancellationToken cancellationToken)
    {
        var record = await Credentials.FindByUsername(username, cancellationToken);
        if (record == null)
            throw new ArgumentException("Invalid username or password", nameof(username));

        return record;
    }

    public async Task<AuthSession> CreateSession(string gatewayId, string username, IPAddress clientAddress,
        CancellationToken cancellationToken)
    {
        var character = await Users.FindByUsername(username, cancellationToken);
        if (character == null)
            throw new NotSupportedException(); // TODO: Create char data?

        var existingSession = await Sessions.FindByUsername(username, cancellationToken);
        if (existingSession != null)
            return existingSession;

        var session = new AuthSession(character)
        {
            GatewayId = gatewayId,
            ServerId  = 0,
            ChannelId = 0,
            Token     = Guid.NewGuid().ToString().ToUpper(),
            Address   = clientAddress,
            LoginTime = DateTime.UtcNow
        };

        await Sessions.Add(session, cancellationToken);
        return session;
    }

    public Task<int> Commit()
    {
        return _context.SaveChangesAsync();
    }
}
