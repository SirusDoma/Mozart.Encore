using System.Net;
using Encore.Data.Entities;
using Encore.Data.Repositories;
using Encore.Services;
using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;

namespace Encore.Contexts;

public interface IAuthContext : IContext
{
    IMemberRepository  Members  { get; }
    IUserRepository    Users    { get; }
    ISessionRepository Sessions { get; }

    Task<Member> FindMember(string username, CancellationToken cancellationToken = default);

    Task<AuthSession> CreateSession(string gatewayId, string username, IPAddress clientAddress,
        CancellationToken cancellationToken = default);
}

public class AuthContext : IAuthContext
{
    private readonly MainDbContext _context;
    private readonly IAuthSessionTokenGenerator _tokenGenerator;

    public AuthContext(IDbContextFactory<MainDbContext> factory, IAuthSessionTokenGenerator tokenGenerator)
    {
        var sharedContextFactory = new SharedDbContextFactory<MainDbContext>(factory);

        _context       = sharedContextFactory.CreateDbContext();
        _tokenGenerator = tokenGenerator;
        Members        = new MemberRepository(sharedContextFactory);
        Users          = new UserRepository(sharedContextFactory);
        Sessions       = new SessionRepository(sharedContextFactory);
    }

    public IMemberRepository  Members  { get; }
    public IUserRepository    Users    { get; }
    public ISessionRepository Sessions { get; }

    public async Task<Member> FindMember(string username, CancellationToken cancellationToken)
    {
        var record = await Members.FindByUsername(username, cancellationToken);
        if (record == null)
            throw new ArgumentException("Invalid username or password", nameof(username));

        return record;
    }

    public async Task<AuthSession> CreateSession(string gatewayId, string username, IPAddress clientAddress,
        CancellationToken cancellationToken)
    {
        var character = await Users.FindByUsername(username, cancellationToken);
        if (character == null)
            throw new NotSupportedException();

        var existingSession = await Sessions.FindByUsername(username, cancellationToken);
        if (existingSession != null)
            return existingSession;

        var session = new AuthSession(character)
        {
            GatewayId = gatewayId,
            ServerId  = 0,
            ChannelId = 0,
            Token     = _tokenGenerator.Generate(),
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
