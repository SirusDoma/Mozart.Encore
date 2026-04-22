namespace Encore.Sessions;

public interface ITcpSessionManager : ITcpSessionManager<TcpSession>
{
}

public interface ITcpSessionManager<TSession> : ISessionManager<TSession>
    where TSession : ITcpSession
{

    Task StopSession(TSession session);

    bool Validate(TSession session);

    Task ClearSessions();
}

public class TcpSessionManager<TSession> : ITcpSessionManager<TSession>
    where TSession : ITcpSession
{
    private class ManagedSession
    {
        public required TSession Session { get; init; }

        public required Task Execution { get; init; }

        public required CancellationTokenSource CancellationTokenSource { get; init; }

    }
    private readonly List<ManagedSession> _sessions = [];

    public event EventHandler<SessionEventArgs<TSession>>? Started;

    public event EventHandler<SessionEventArgs<TSession>>? Stopped;

    public event EventHandler<SessionErrorEventArgs<TSession>>? Error;

    public TcpSessionManager()
    {
    }

    public virtual void StartSession(TSession session)
    {
        if (Validate(session))
            return;

        session.Disconnected += OnSessionDisconnected;

        var cancellationTokenSource = new CancellationTokenSource();
        var execution = Task.Run(async () =>
        {
            try
            {
                await session.Execute(cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new SessionErrorEventArgs<TSession>()
                {
                    Session = session,
                    Exception = ex
                });

                await StopSession(session);
            }
            finally
            {
                session.Terminate();
            }
        }, cancellationTokenSource.Token);

        _sessions.Add(new ManagedSession
        {
            Session = session,
            Execution = execution,
            CancellationTokenSource = cancellationTokenSource
        });

        Started?.Invoke(this, new SessionEventArgs<TSession> { Session = session });
    }

    public virtual Task StopSession(TSession session)
    {
        var managed = _sessions.FirstOrDefault(m => ReferenceEquals(m.Session, session));
        if (managed == null)
            return Task.CompletedTask;

        managed.Session.Disconnected -= OnSessionDisconnected;
        managed.CancellationTokenSource.Cancel();

        Stopped?.Invoke(this, new SessionEventArgs<TSession> { Session = session });
        _sessions.Remove(managed);

        return managed.Execution;
    }

    public virtual bool Validate(TSession session)
    {
        return _sessions.FirstOrDefault(m => ReferenceEquals(m.Session, session)) != null;
    }

    public Task ClearSessions()
    {
        List<Task> tasks = [];
        foreach (var managed in _sessions)
        {
            managed.Session.Disconnected -= OnSessionDisconnected;
            managed.CancellationTokenSource.Cancel();

            tasks.Add(managed.Execution);
        }

        _sessions.Clear();
        return Task.WhenAll(tasks);
    }

    private void OnSessionDisconnected(object? sender, EventArgs e)
    {
        StopSession((TSession)sender!);
    }

    public virtual void Dispose()
        => ClearSessions();
}

public sealed class TcpSessionManager : TcpSessionManager<TcpSession>
{
}
