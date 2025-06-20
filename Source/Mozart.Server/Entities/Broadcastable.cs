using Encore.Messaging;
using Mozart.Sessions;

namespace Mozart.Entities;

public interface IBroadcastable
{
    IReadOnlyList<Session> Sessions { get; }

    Task<int> Broadcast<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage;

    Task<int> Broadcast<TMessage>(Session sender, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage;

    Task<int> Broadcast<TMessage>(Func<Session, bool> predicate, TMessage message,
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

    public async Task<int> Broadcast<TMessage>(Func<Session, bool> predicate, TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        var sessions = Sessions.Where(predicate).ToList();
        if (sessions.Count == 0)
            return 0;

        await Task.WhenAll(sessions.Select(s =>
            WriteMessageFrame(s, message, cancellationToken)
        ));

        return sessions.Count;
    }

    private static async Task WriteMessageFrame<TMessage>(Session session, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        await session.WriteMessage(message, cancellationToken);
    }
}