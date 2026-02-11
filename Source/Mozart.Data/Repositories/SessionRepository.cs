using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Data.Entities;

namespace Mozart.Data.Repositories;

public interface ISessionRepository : IRepository<AuthSession>
{
    Task<AuthSession?> Find(string token, CancellationToken cancellationToken = default);

    Task<AuthSession?> FindByUsername(string username, CancellationToken cancellationToken = default);

    Task<bool> Check(string token, CancellationToken cancellationToken = default);

    Task<bool> CheckByUsername(string username, CancellationToken cancellationToken = default);

    Task Add(AuthSession session, CancellationToken cancellationToken = default);

    Task UpdateChannel(string token, int serverId, int channelId, CancellationToken cancellationToken = default);

    Task Revoke(string token, CancellationToken cancellationToken = default);

    Task Clear(CancellationToken cancellationToken = default);

    Task Clear(int serverId, int channelId, CancellationToken cancellationToken = default);
}

public class SessionRepository(IDbContextFactory<UserDbContext> factory)
    : Repository<AuthSession, UserDbContext>(factory.CreateDbContext()), ISessionRepository
{
    public Task<AuthSession?> Find(string token, CancellationToken cancellationToken)
    {
        return DbSet.AsNoTracking().SingleOrDefaultAsync(c => c.Token == token, cancellationToken);
    }

    public Task<AuthSession?> FindByUsername(string username, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().SingleOrDefaultAsync(c => c.Username == username, cancellationToken);
    }

    public Task<bool> Check(string token, CancellationToken cancellationToken)
    {
        return DbSet.AsNoTracking().AnyAsync(c => c.Token == token, cancellationToken);
    }

    public Task<bool> CheckByUsername(string username, CancellationToken cancellationToken)
    {
        return DbSet.AnyAsync(c => c.Username == username, cancellationToken);
    }

    public Task Add(AuthSession session, CancellationToken cancellationToken)
    {
        Context.Sessions.Add(session);
        return Task.CompletedTask;
    }

    public Task UpdateChannel(string token, int serverId, int channelId, CancellationToken cancellationToken)
    {
        return Context.Sessions
            .Where(e => e.Token == token)
            .ExecuteUpdateAsync(p => p
                    .SetProperty(s => s.ServerId,  _ => serverId)
                    .SetProperty(s => s.ChannelId, _ => channelId),
                cancellationToken);
    }

    public Task Revoke(string token, CancellationToken cancellationToken = default)
    {
        return Context.Sessions
            .Where(e => e.Token == token)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task Clear(CancellationToken cancellationToken)
    {
        await DbSet.ExecuteDeleteAsync(cancellationToken);
    }

    public async Task Clear(int serverId, int channelId, CancellationToken cancellationToken = default)
    {
        await DbSet.Where(s => s.ServerId == serverId && s.ChannelId == channelId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
