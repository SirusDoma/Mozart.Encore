namespace Memoryer.Relay.Sessions;

public class GameSession
{
    public required int Id { get; init; }

    public required IReadOnlyList<RelaySession> Sessions { get; init; }
}
