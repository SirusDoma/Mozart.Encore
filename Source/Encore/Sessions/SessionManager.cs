namespace Encore.Sessions;

public interface ISessionManager<in TSession>: IDisposable
    where TSession : Session
{
    void StartSession(TSession session);

    Task StopSession(TSession session);

    bool Validate(TSession session);

    Task ClearSessions();
}

public interface ISessionManager : ISessionManager<Session>
{
}

public class SessionEventArgs : EventArgs
{
    public required Session Session { get; init; }

    public static implicit operator SessionEventArgs(Session session)
    {
        return new SessionEventArgs { Session = session };
    }
}

public sealed class SessionErrorEventArgs : SessionEventArgs
{
    public required Exception Exception { get; init; }
}

public class SessionManager<TSession> : ISessionManager<TSession>
    where TSession : Session
{
    private class ManagedSession
    {
        public required TSession Session { get; init; }

        public required Task Execution { get; init; }

        public required CancellationTokenSource CancellationTokenSource { get; init; }

    }
    private readonly List<ManagedSession> _sessions = [];

    public event EventHandler<SessionEventArgs>? Started;

    public event EventHandler<SessionEventArgs>? Stopped;

    public event EventHandler<SessionErrorEventArgs>? Error;

    public SessionManager()
    {
    }

    public void StartSession(TSession session)
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
                Error?.Invoke(this, new SessionErrorEventArgs
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

        Started?.Invoke(this, session);
    }

    public Task StopSession(TSession session)
    {
        var managed = _sessions.FirstOrDefault(m => m.Session == session);
        if (managed == null)
            return Task.CompletedTask;

        managed.Session.Disconnected -= OnSessionDisconnected;
        managed.CancellationTokenSource.Cancel();

        Stopped?.Invoke(this, session);
        _sessions.Remove(managed);

        return managed.Execution;
    }

    public bool Validate(TSession session)
    {
        return _sessions.FirstOrDefault(m => m.Session == session) != null;
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

public sealed class SessionManager : SessionManager<Session>
{
}
