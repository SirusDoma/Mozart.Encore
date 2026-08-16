using Encore.Entities;
using Encore.Server;
using Encore.Server.Sessions;
using Identity.Controllers.Filters;
using Identity.Messages.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mozart.Data.Contexts;
using Mozart.Metadata;
using Mozart.Sessions;

namespace Identity.Controllers;

[ChannelAuthorize]
public class RankingController(
    Session session,
    MainDbContext context,
    ILogger<RankingController> logger
) : CommandController<Session>(session)
{
    private IChannel Channel => Session.Channel!;

    [CommandHandler(RequestCommand.GetMusicPlayRanking)]
    public async Task<MusicPlayRankingResponse> GetMusicPlayRanking(CancellationToken cancellationToken)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation((int)RequestCommand.GetMusicPlayRanking, "Get music play ranking: [{User}]",
            actor.Nickname);

        var entries = await context.UserRankingsExtended
            .OrderBy(r => r.Ranking)
            .Take(100)
            .Select(r => new MusicPlayRankingResponse.RankEntry
            {
                Rank          = r.Ranking,
                Nickname      = r.Nickname,
                Battles       = r.Battle,
                Wins          = r.Win,
                WinRate       = r.Battle > 0 ? (int)((float)r.Win / r.Battle) : 0,
                RankDeltaType = r.ChangeType == 0 ? RankDeltaType.Down : RankDeltaType.Up,
                RankDelta     = r.ChangeRanking
            })
            .ToListAsync(cancellationToken);

        return new MusicPlayRankingResponse
        {
            Self = new MusicPlayRankingResponse.RankEntry
            {
                Rank          = actor.Ranking,
                Nickname      = actor.Nickname,
                Battles       = actor.Battle,
                Wins          = actor.Win,
                WinRate       = actor.Battle > 0 ? (int)((float)actor.Win / actor.Battle) : 0,
                RankDeltaType = actor.RankDeltaType,
                RankDelta     = actor.RankDelta
            },
            Entries = entries
        };
    }

    [CommandHandler(RequestCommand.GetMusicScoreList)]
    public MusicScoreListResponse GetMusicScoreList()
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation(
            (int)RequestCommand.GetMusicScoreList,
            "Get music score list"
        );

        return new MusicScoreListResponse
        {
            MaxScores = actor.MusicScoreRecords
                .GroupBy(r => r.MusicId)
                .Select(g =>
                {
                    Session.Channel!.GetMusicList().TryGetValue(g.Key, out var music);

                    // The client reads a fixed-size record: one score and one rank per difficulty
                    var records = g.ToDictionary(r => r.Difficulty);
                    return new MusicScoreListResponse.MusicScoreEntry
                    {
                        MusicId = (ushort)g.Key,
                        Scores  = MusicScoreListResponse.Difficulties.ToDictionary(d => d,
                            d => records.TryGetValue(d, out var r) ? (int)r.Score : 0),
                        Ranks   = MusicScoreListResponse.Difficulties.ToDictionary(d => d,
                            d => records.TryGetValue(d, out var r)
                                 ? RankEvaluator.Evaluate(r.Score, d, music, r.ClearType)
                                 : default)
                    };
                })
                .ToList()
        };
    }
}
