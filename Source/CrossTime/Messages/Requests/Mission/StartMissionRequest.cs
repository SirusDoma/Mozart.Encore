using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class StartMissionRequest : IMessage
{
    public static Enum Command => RequestCommand.StartMission;

    [MessageField(order: 0)]
    public int MusicId { get; init; }

    [MessageField(order: 1)]
    public int MissionLevel { get; init; }
}
