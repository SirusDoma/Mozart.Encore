using Encore.Messaging;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IBroadcastable
{
    public IReadOnlyList<Session> Sessions { get; }

    public Task<int> Broadcast<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage;

    public Task<int> Broadcast<TContext, TMessage>(TContext ctx, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage;

    public Task<int> Broadcast<TMessage>(Session sender, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage;

    public Task<int> Broadcast<TContext, TMessage>(Session sender, TContext ctx, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage;
}

public abstract class Broadcastable : IBroadcastable
{
    public abstract IReadOnlyList<Session> Sessions { get; }

    public virtual async Task<int> Broadcast<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        await Task.WhenAll(Sessions.Select(s =>
            WriteMessageFrame(s, message, cancellationToken)
        ));

        return Sessions.Count;
    }

    public virtual async Task<int> Broadcast<TContext, TMessage>(TContext ctx, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        var sessions = GetSessionsByContext(ctx).ToList();
        if (sessions.Count == 0)
            return 0;

        await Task.WhenAll(sessions.Select(s =>
            WriteMessageFrame(s, message, cancellationToken)
        ));

        return sessions.Count;
    }

    public virtual async Task<int> Broadcast<TMessage>(Session sender, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        var sessions = Sessions.Where(s => s != sender).ToList();
        if (sessions.Count == 0)
            return 0;

        await Task.WhenAll(sessions.Select(s =>
            WriteMessageFrame(s, message, cancellationToken)
        ));

        return sessions.Count;
    }

    public virtual async Task<int> Broadcast<TContext, TMessage>(Session sender, TContext ctx, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        var sessions = GetSessionsByContext(ctx).Where(s => s != sender).ToList();
        if (sessions.Count == 0)
            return 0;

        await Task.WhenAll(sessions.Select(s =>
            WriteMessageFrame(s, message, cancellationToken)
        ));

        return sessions.Count;
    }

    private async Task WriteMessageFrame<TMessage>(Session session, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        await session.WriteMessage(message, cancellationToken);
    }

    protected abstract IEnumerable<Session> GetSessionsByContext<TContext>(TContext ctx);
}