using Memoryer.Relay.Sessions;

namespace Memoryer.Relay.Services;

public interface IGameSessionService
{
    GameSession CreateGameSession(IEnumerable<IRelayPeer> sessions);

    GameSession? DeleteGameSession(int id);

    GameSession? GetGameSession(int id);

    IReadOnlyList<GameSession> GetGameSessions();
}
