namespace Encore.Sessions;

public interface ISessionManager<TSession>: IDisposable
    where TSession : ISession
{
    public event EventHandler<SessionEventArgs<TSession>>? Started;

    public event EventHandler<SessionEventArgs<TSession>>? Stopped;

    public event EventHandler<SessionErrorEventArgs<TSession>>? Error;

    void StartSession(TSession session);
}

public interface ISessionManager : ISessionManager<ISession>
{
}

public class SessionEventArgs<TSession> : EventArgs
    where TSession : ISession
{
    public required TSession Session { get; init; }
}


public class SessionEventArgs : SessionEventArgs<ISession>
{
}

public class SessionErrorEventArgs<TSession> : SessionEventArgs<TSession>
    where TSession : ISession
{
    public required Exception Exception { get; init; }
}


public sealed class SessionErrorEventArgs : SessionErrorEventArgs<ISession>
{
}
