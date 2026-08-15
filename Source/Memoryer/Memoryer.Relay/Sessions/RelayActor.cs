using System.Net;

namespace Memoryer.Relay.Sessions;

public class RelayActor
{
    public int SessionKey1 { get; set; }
    public int SessionKey2 { get; set; }

    public required IPEndPoint RelayServer    { get; set; }
    public IPEndPoint LocalEndpoint  { get; set; } = new(IPAddress.None, 0);
    public IPEndPoint PublicEndpoint { get; set; } = new(IPAddress.None, 0);
    public int GameSessionId { get; set; }
}
