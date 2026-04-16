using Encore.Server;
using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Entities;
using Mozart.Sessions;
using Identity.Controllers.Filters;
using Identity.Messages.Responses;

using Microsoft.Extensions.Logging;
using Mozart.Metadata;

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
        logger.LogInformation((int)RequestCommand.GetCharacterInfo, "Get music play ranking: [{User}]",
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
                    return new MusicScoreListResponse.MusicScoreEntry
                    {
                        MusicId = (ushort)g.Key,
                        Scores  = g.ToDictionary(r => r.Difficulty, r => (int)r.Score),
                        Ranks   = g.ToDictionary(r => r.Difficulty, r => RankEvaluator.Evaluate(r.Score, r.Difficulty, music, r.ClearType))
                    };
                })
                .ToList()
        };
    }
}
