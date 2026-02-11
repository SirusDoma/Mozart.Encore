using System.Net;

namespace Mozart.Data.Entities;

public class AuthSession
{
    public AuthSession()
    {
    }

    public AuthSession(User user)
    {
        UserId    = user.Id;
        Username  = user.Username;
    }

    public required string GatewayId { get; init; }

    public required int ServerId { get; init; }

    public required int ChannelId { get; init; }

    public int UserId { get; init; }

    public string Username { get; init; } = null!;

    public required string Token { get; init; }

    public required IPAddress Address { get; init; }

    public DateTime LoginTime { get; init; } = DateTime.UtcNow;
}
