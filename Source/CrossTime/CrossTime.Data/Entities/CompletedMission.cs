using Mozart.Metadata;

namespace Mozart.Data.Entities;

public class CompletedMission
{
    public int UserId { get; init; }

    public int GatewayId { get; init; }

    public int SetId { get; init; }

    public int Level { get; init; }

    public Rank Rank { get; set; }
}
