using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class MissionRanksRequest : IMessage
{
    public static Enum Command => RequestCommand.MissionRanks;

    [MessageField(order: 0)]
    public int MissionSetId { get; init; }
}
