using Encore.Messaging;

namespace CrossTime.Messages.Responses;

public class StartMissionResponse : IMessage
{
    public static Enum Command => ResponseCommand.StartMission;

    public enum StartMissionResult : uint
    {
        Success            = 0x00000000, // 0
        InsufficientGem    = 0xFFFFFFFA, // -6
        InsufficientTicket = 0xFFFFFFFB, // -5
        TooManyTicket      = 0xFFFFFFFC, // -4
        InvalidLevel       = 0xFFFFFFFD, // -3
        SuspiciousUser     = 0xFFFFFFFE, // -2
        InvalidPlanet      = 0xFFFFFFFF, // -1
    }

    public enum SpendingType : uint
    {
        Gem    = 0x00000000, // 0
        Ticket = 0x00000001, // 1
    }

    [MessageField(order: 0)]
    public StartMissionResult Result { get; init; }

    [MessageField(order: 1)]
    public SpendingType Currency { get; init; }

    [MessageField(order: 2)]
    public int Value { get; init; }
}
