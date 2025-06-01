using Encore.Messaging;

namespace Mozart;

public class SubmitScoreRequest : IMessage
{
    public static Enum Command => RequestCommand.SubmitScore;

    [MessageField(order: 0)]
    public ushort Cool { get; init; }

    [MessageField(order: 1)]
    public ushort Good { get; init; }

    [MessageField(order: 2)]
    public ushort Bad { get; init; }

    [MessageField(order: 3)]
    public ushort Miss { get; init; }

    [MessageField(order: 4)]
    public ushort MaxCombo { get; init; }

    [MessageField(order: 5)]
    public ushort JamCombo { get; init; }

    [MessageField(order: 6)]
    public ushort MaxJamCombo { get; init; }

    [MessageField(order: 7)]
    public uint Score { get; init; }

    [MessageField(order: 8)]
    public byte Health { get; init; }

    public bool Clear => Health > 0;
}