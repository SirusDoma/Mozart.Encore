using Encore.Server;
using Microsoft.EntityFrameworkCore;
using Mozart.Data.Contexts;
using Mozart.Entities;
using Mozart.Sessions;
using Memoryer.Controllers.Filters;
using Memoryer.Messages.Requests;
using Memoryer.Messages.Responses;
using Microsoft.Extensions.Logging;
using Mozart.Data.Entities;
using Mozart.Metadata;

namespace Memoryer.Controllers;

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
                    var scores = g.ToDictionary(r => r.Difficulty);
                    return new MusicScoreListResponse.MusicScoreEntry
                    {
                        MusicId = (ushort)g.Key,
                        Ranks   = Enum.GetValues<Difficulty>().ToDictionary(d => d, d =>
                            scores.TryGetValue(d, out var r)
                                ? RankEvaluator.Evaluate(r.Score, r.Difficulty, music, r.ClearType)
                                : Rank.None
                        )
                    };
                })
                .ToList()
        };
    }

    [CommandHandler]
    public MusicMaxScoreResponse GetMusicMaxScore(MusicMaxScoreRequest request)
    {
        var actor = Session.GetAuthorizedToken<Actor>();
        logger.LogInformation(
            (int)RequestCommand.GetMusicMaxScore,
            "Get music max score"
        );

        var diff = Difficulty.EX;
        if (Channel.GetMusicList().TryGetValue(request.MusicId, out var music))
        {
            if (music.NoteCountEx == request.NoteCount)
                diff = Difficulty.EX;
            else if (music.NoteCountNx == request.NoteCount)
                diff = Difficulty.NX;
            else if (music.NoteCountHx == request.NoteCount)
                diff = Difficulty.HX;
        }

        return new MusicMaxScoreResponse
        {
            MaxScore = actor.MusicScoreRecords
                .FirstOrDefault(s => s.MusicId == request.MusicId && s.Difficulty == diff)?.Score ?? 0
        };
    }
}
