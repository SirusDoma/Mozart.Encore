using System.Collections.Concurrent;
using Encore.Sessions;

namespace Mozart.Sessions;

public interface ISessionManager : ISessionManager<Session>
{
    void StartExpiry(Session session, TimeSpan expiry, Action<Session>? callback = null);
    bool CancelExpiry(Session session);
}

public class SessionManager : SessionManager<Session>, ISessionManager
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _expiryCancellations = new();

    public void StartExpiry(Session session, TimeSpan expiry, Action<Session>? callback = null)
    {
        if (!Validate(session))
            return;

        if (!session.Authorized)
            return;

        string token = session.Actor.Token;
        if (_expiryCancellations.TryRemove(token, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
        }

        var cts = new CancellationTokenSource();
        _expiryCancellations[token] = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(expiry, cts.Token);

                _expiryCancellations.TryRemove(token, out _);
                callback?.Invoke(session);

                await base.StopSession(session);
            }
            catch (OperationCanceledException)
            {
                // Expiry was canceled
            }
            finally
            {
                cts.Dispose();
            }
        }, cts.Token);
    }

    public bool CancelExpiry(Session session)
    {
        if (!session.Authorized)
            return false;

        if (_expiryCancellations.TryRemove(session.Actor.Token, out var cts))
        {
            cts.Cancel();
            cts.Dispose();

            return true;
        }

        return false;
    }

    public new async Task StopSession(Session session)
    {
        CancelExpiry(session);
        await base.StopSession(session);
    }

    public new async Task ClearSessions()
    {
        foreach (var cts in _expiryCancellations.Values)
        {
            await cts.CancelAsync();
            cts.Dispose();
        }
        _expiryCancellations.Clear();

        await base.ClearSessions();
    }

    public override void Dispose()
    {
        GC.SuppressFinalize(this);

        foreach (var cts in _expiryCancellations.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _expiryCancellations.Clear();
        
        base.Dispose();
    }
}
