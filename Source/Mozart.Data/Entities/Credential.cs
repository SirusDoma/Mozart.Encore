namespace Mozart.Data.Entities;

public class Credential
{
    public long Id { get; init; }

    public int UserId { get; init; }

    public required string Username { get; init; }

    public required byte[] Password { get; init; }
}