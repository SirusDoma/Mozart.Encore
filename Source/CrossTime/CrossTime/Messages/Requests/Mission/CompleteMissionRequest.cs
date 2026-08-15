using Encore.Messaging;

namespace CrossTime.Messages.Requests;

public class CompleteMissionRequest : IMessage
{
    public static Enum Command => RequestCommand.CompleteMission;

    [MessageField(order: 0)]
    public int MissionSetId { get; init; }

    [MessageField(order: 1)]
    public int Score { get; init; }

    [MessageField(order: 2)]
    public int Cool { get; init; }

    [MessageField(order: 3)]
    public int Good { get; init; }

    [MessageField(order: 4)]
    public int Bad { get; init; }

    [MessageField(order: 5)]
    public int Miss { get; init; }

    [MessageField(order: 6)]
    public int MaxJamCombo { get; init; }

    [MessageField(order: 7)]
    public int MaxCombo { get; init; }

    [MessageField(order: 8)]
    public int MaxScore { get; init; }

    [MessageField(order: 9)]
    public int MaxNotes { get; init; }
}
