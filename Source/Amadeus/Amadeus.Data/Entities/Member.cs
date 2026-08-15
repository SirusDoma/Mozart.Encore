namespace Mozart.Data.Entities;

public class Member
{
    public long Id { get; init; }

    public required string Username { get; init; }

    public required byte[] Password { get; init; }
}
