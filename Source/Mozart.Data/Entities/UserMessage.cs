using Mozart.Metadata;

namespace Mozart.Data.Entities;

public class UserMessage
{
    public int Id { get; init; }

    public required string SenderUsername { get; init; }

    public required int SenderId { get; init; }

    public required string SenderNickname { get; init; }

    public required string ReceiverUsername { get; init; }

    public required int ReceiverId { get; init; }

    public required string ReceiverNickname { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public DateTime WriteDate { get; init; }

    public DateTime? ReadDate { get; set; }

    public bool IsRead { get; set; }

    public required GiftType GiftType { get; init; }
}
