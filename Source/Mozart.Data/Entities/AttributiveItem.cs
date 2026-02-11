namespace Mozart.Data.Entities;

public class AttributiveItem
{
    public int Id { get; init; }

    public required int UserId { get; init; }

    public required int ItemId { get; init; }

    public required int Count { get; set; }

    public required int PreviousCount { get; set; }

    public required DateTime AcquiredAt { get; init; } = DateTime.UtcNow;
}
