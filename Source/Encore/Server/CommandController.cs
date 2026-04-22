using Encore.Sessions;

namespace Encore.Server;

public abstract class CommandController
{
    protected CommandController(ISession session)
    {
        Session = session;
    }

    protected virtual ISession Session { get; }
}

public abstract class CommandController<TSession> : CommandController
    where TSession : ISession
{
    protected CommandController(TSession session) :
        base(session)
    {
        Session = session;
    }

    protected new TSession Session { get; }
}
