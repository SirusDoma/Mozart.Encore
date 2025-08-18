using System.Runtime.ExceptionServices;
using Encore.Messaging;
using Session = Mozart.Sessions.Session;

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

    void Invalidate();
}

public abstract class Broadcastable : IBroadcastable
{
    public abstract IReadOnlyList<Session> Sessions { get; }

    public virtual async Task<int> Broadcast<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        await Broadcast(Sessions.ToList(), message, cancellationToken);
        return Sessions.Count;
    }

    public virtual async Task<int> Broadcast<TMessage>(Session sender, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        var sessions = Sessions.Where(s => s != sender).ToList();
        if (sessions.Count == 0)
            return 0;

        await Broadcast(sessions, message, cancellationToken);
        return sessions.Count;
    }

    public async Task<int> Broadcast<TMessage>(Func<Session, bool> predicate, TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        var sessions = Sessions.Where(predicate).ToList();
        if (sessions.Count == 0)
            return 0;

        await Broadcast(sessions, message, cancellationToken);
        return sessions.Count;
    }

    protected virtual async Task WriteMessageFrame<TMessage>(Session session, TMessage message,
        CancellationToken cancellationToken) where TMessage : class, IMessage
    {
        await session.WriteMessage(message, cancellationToken);
    }

    private async Task Broadcast<TMessage>(ICollection<Session> sessions, TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        var task = Task.WhenAll(sessions.Select(s => WriteMessageFrame(s, message, cancellationToken)));

        try
        {
            await task;
        }
        catch (Exception)
        {
            Invalidate();
            if (task.Exception != null)
                ExceptionDispatchInfo.Throw(task.Exception);
            else
                throw;
        }
    }

    public abstract void Invalidate();
}