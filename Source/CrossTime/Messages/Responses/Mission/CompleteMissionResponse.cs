using Encore.Messaging;
using MongoDB.Bson;
using Mozart.Metadata;

namespace CrossTime.Messages.Responses;

public class CompleteMissionResponse : IMessage
{
    public static Enum Command => ResponseCommand.CompleteMission;

    public class MissionFinalInfo : SubMessage
    {
        [MessageField<MessageFieldCodec<int>>(order: 0)]
        public bool Invalid { get; init; } = false;

        [MessageField(order: 1)]
        public int Level { get; init; }

        [MessageField(order: 2)]
        public int BonusGemStar { get; init; }
    }

    public enum ErrorCode : uint
    {
        None    = 0x00000000, // 00
        Error   = 0xFFFFFFFE, // -2: To be combined with `NotRecorded` or `InvalidMission`
        Invalid = 0xFFFFFFFD  // -3
    }

    public enum MissionResult : uint
    {
        Success        = 0x00000000, // 00
        NotRecorded    = 0x00000001, // 01
        Final          = 0x00000002, // 02: To be combined with `FinalInfo`
        InvalidMission = 0xFFFFFFFF  // -1 (or could be anything other than 1)
    }

    [MessageField(order: 0)]
    public ErrorCode Error { get; init; } = ErrorCode.None;

    [MessageField(order: 1)]
    public MissionResult Result { get; init; }

    [MessageField(order: 2)]
    public int MissionSetId { get; init; }

    [MessageField(order: 3)]
    public int MissionLevel { get; init; }

    [MessageField(order: 4)]
    public Rank Rank { get; init; }

    [MessageField(order: 5)]
    public Rank BestRank { get; init; }

    [MessageField(order: 6)]
    public int Level { get; init; }

    [MessageField(order: 7)]
    public int RewardGemStar { get; init; }

    [MessageField(order: 8)]
    public int TotalGemStar { get; init; }

    [MessageField(order: 9)]
    public MissionFinalInfo? FinalInfo { get; init; } = null;
}
