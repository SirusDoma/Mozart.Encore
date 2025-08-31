using Encore.Messaging;

namespace Amadeus.Messages.Events;

public class StartGameEventData : IMessage
{
    public enum StartResult : uint
    {
        Success             = 0x00000000, // 0
        NotReady            = 0x00000001, // 1
        TeamUnbalanced      = 0x00000002, // 2
        InsufficientPlayers = 0x00000003, // 3
    }

    public static Enum Command => EventCommand.StartGame;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public StartResult Result { get; init; } = StartResult.Success;

    [MessageField(order: 1)]
    public int SkillsSeed { get; init; }
}