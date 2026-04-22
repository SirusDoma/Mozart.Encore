using System.Net;

namespace Memoryer.Relay.Sessions;

public class RelayActor
{
    public int SessionKey1 { get; init; }
    public int SessionKey2 { get; init; }

    public required IPEndPoint RelayServer    { get; init; }
    public IPEndPoint LocalEndpoint  { get; set; } = new(IPAddress.None, 0);
    public IPEndPoint PublicEndpoint { get; set; } = new(IPAddress.None, 0);
    public int GameSessionId { get; set; }
}
