using System.Collections.Concurrent;
using Memoryer.Relay.Sessions;
using Microsoft.Extensions.Logging;

namespace Memoryer.Relay.Services;

public class GameSessionEventArgs : EventArgs
{
    public required GameSession Session { get; init; }
}

public class GameSessionService : IGameSessionService
{
    private readonly ConcurrentDictionary<int, GameSession> _sessions = new();
    private readonly ILogger<GameSessionService> _logger;
    private int _nextId;

    public event EventHandler<GameSessionEventArgs>? SessionCreated;
    public event EventHandler<GameSessionEventArgs>? SessionDeleted;

    public GameSessionService(ILogger<GameSessionService> logger)
    {
        _logger = logger;
    }

    public GameSession CreateGameSession(IEnumerable<IRelayPeer> sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);

        var members = sessions.ToList();
        if (members.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(sessions), "At least one session is required");

        int id = Interlocked.Increment(ref _nextId);
        if (id == 0)
            id = Interlocked.Increment(ref _nextId);

        var session = new GameSession
        {
            Id       = id,
            Sessions = members
        };

        if (!_sessions.TryAdd(id, session))
            throw new InvalidOperationException($"GameSession id '{id}' already exists");

        foreach (var member in members)
            member.GetAuthorizedToken<RelayActor>().GameSessionId = id;

        SessionCreated?.Invoke(this, new GameSessionEventArgs { Session = session });
        return session;
    }

    public GameSession? DeleteGameSession(int id)
    {
        if (!_sessions.TryRemove(id, out var session))
            return null;

        SessionDeleted?.Invoke(this, new GameSessionEventArgs { Session = session });
        return session;
    }

    public GameSession? GetGameSession(int id)
    {
        return _sessions.TryGetValue(id, out var session) ? session : null;
    }

    public IReadOnlyList<GameSession> GetGameSessions()
    {
        return _sessions.Values.ToList();
    }
}
