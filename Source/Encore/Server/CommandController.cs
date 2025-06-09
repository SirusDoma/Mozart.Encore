using Encore.Sessions;

namespace Encore.Server;

public abstract class CommandController
{
    protected CommandController(Session session)
    {
        Session = session;
    }

    protected virtual Session Session { get; }
}

public abstract class CommandController<TSession> : CommandController
    where TSession : Session
{
    protected CommandController(TSession session) :
        base(session)
    {
        Session = session;
    }

    protected override TSession Session { get; }
}