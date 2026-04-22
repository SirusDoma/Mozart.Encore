namespace Encore.Sessions;

public interface IUdpSessionManager : IUdpSessionManager<UdpSession>
{
}

public interface IUdpSessionManager<TSession> : ISessionManager<TSession>
    where TSession : IUdpSession
{
}

public class UdpSessionManager<TSession> : IUdpSessionManager<TSession>
    where TSession : class, IUdpSession
{
    public event EventHandler<SessionEventArgs<TSession>>? Started;

    public event EventHandler<SessionEventArgs<TSession>>? Stopped;

    public event EventHandler<SessionErrorEventArgs<TSession>>? Error;

    public UdpSessionManager()
    {
    }

    public virtual void StartSession(TSession session)
    {
        Started?.Invoke(this, new SessionEventArgs<TSession> { Session = session });

        // Fire-and-forget: a UDP session is single-shot (one Dispatch then done).
        // Errors propagate to the Error event; the receive loop is never blocked.
        _ = Task.Run(async () =>
        {
            try
            {
                await session.Execute(CancellationToken.None).ConfigureAwait(false);
                Stopped?.Invoke(this, new SessionEventArgs<TSession> { Session = session });
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new SessionErrorEventArgs<TSession>
                {
                    Session = session,
                    Exception = ex
                });
            }
        });
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public sealed class UdpSessionManager : UdpSessionManager<UdpSession>
{
}
