namespace Mozart.Data.Entities;

public class GiftItem
{
    public int Id { get; init; }

    public required int UserId { get; init; }

    public required int ItemId { get; init; }

    public required int SenderId { get; init; }

    public required string SenderNickname { get; init; }

    public DateTime SendDate { get; init; }
}