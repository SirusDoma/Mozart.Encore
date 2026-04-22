using System.Net;

namespace Mozart.Sessions;

public class RelaySessionInfo
{
    public int RelaySessionKey1 { get; set; }
    public int RelaySessionKey2 { get; set; }

    public required IPEndPoint PublicEndpoint { get; init; }
    public required IPEndPoint LocalEndpoint  { get; init; }
}
