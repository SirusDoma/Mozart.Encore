using Encore.Messaging;
using Encore.Server;
using Encore.Sessions;

namespace Mozart;

[Authorize]
public class PlayingController(Session session, IMessageCodec codec)
    : CommandController(session)
{
    [CommandHandler]
    public async Task<GameStatsUpdateEventData> UpdateGameStats(UpdateGameStatsRequest request,
        CancellationToken cancellationToken)
    {
        if (request is { Type: StatsType.Health, Value: < 200 })
        {
            await Session.WriteFrameAsync(
                codec.Encode(new GameStatsUpdateEventData()
                {
                    MemberId = 1,
                    Type     = request.Type,
                    Value    = 0
                }),
                cancellationToken
            );
        }

        return new GameStatsUpdateEventData
        {
            MemberId = 0,
            Type     = request.Type,
            Value    = request.Value
        };
    }

    [CommandHandler]
    public Task<ScoreSubmissionEventData> SubmitScore(SubmitScoreRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ScoreSubmissionEventData()
        {
            MemberId = 0,
        });
    }

    [CommandHandler]
    public Task<GameCompletedEventData> DispatchCompletedEvent(SubmitScoreRequest request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new GameCompletedEventData
        {
            Scores =
            [
                new GameCompletedEventData.ScoreEntry
                {
                    MemberId   = 0,
                    Active     = true,
                    Cool       = request.Cool,
                    Good       = request.Good,
                    Bad        = request.Bad,
                    Miss       = request.Miss,
                    MaxCombo   = request.MaxCombo,
                    JamCombo   = request.JamCombo,
                    Score      = request.Score,
                    Reward     = 5000,
                    Level      = 100,
                    Experience = 89500,
                    Win        = request.Score > 1000,
                    Safe       = false
                },
                new GameCompletedEventData.ScoreEntry
                {
                    MemberId    = 1,
                    Active      = true,
                    Cool        = request.Cool,
                    Good        = request.Good,
                    Bad         = request.Bad,
                    Miss        = request.Miss,
                    MaxCombo    = request.JamCombo,
                    JamCombo    = request.MaxJamCombo,
                    Score       = (uint)Math.Max((int)request.Score - 10, 1000),
                    Reward      = 0,
                    Level       = 0,
                    Experience  = 0,
                    Win         = true,
                    Safe        = false
                }
            ]
        });
    }

    [CommandHandler(RequestCommand.ExitPlaying)]
    public async Task<PlayerLeaveGameEventData> ExitPlaying(CancellationToken cancellationToken)
    {
        await Session.WriteFrameAsync(
            codec.Encode(new PlayerLeaveGameEventData()
            {
                MemberId = 1,
                Level = 999
            }),
            cancellationToken
        );

        return new PlayerLeaveGameEventData
        {
            MemberId = 0,
            Level    = 100
        };
    }
}