using CrossTime.Controllers.Filters;
using CrossTime.Messages.Requests;
using CrossTime.Messages.Responses;
using Encore.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Data.Entities;
using Mozart.Data.Repositories;
using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Options;
using Mozart.Services;
using Mozart.Sessions;

namespace CrossTime.Controllers;

[ChannelAuthorize]
public class MissionController(
    Session session,
    IMissionTracker tracker,
    IUserRepository repository,
    IOptions<GameOptions> gameOptions,
    IOptions<GatewayOptions> gatewayOptions,
    ILogger<MainRoomController> logger
) : CommandController<Session>(session)
{

    private IChannel Channel => Session.Channel!;

    [CommandHandler]
    public MissionRanksResponse GetMissionLogs(MissionRanksRequest request)
    {
        var actor = Session.Actor;
        logger.LogInformation(
            (int)RequestCommand.MissionRanks,
            "Get mission logs: [{ServerId}/{channelId:00} - {SetId:00}]",
            actor.ServerId, Channel.Id, request.MissionSetId
        );

        var missions = actor.CompletedMissionList
            .Where(m => m.GatewayId == actor.ServerId && m.SetId == request.MissionSetId)
            .ToList();

        var rankList = missions
            .Select(m => new MissionRanksResponse.RankEntry
            {
                MissionLevel = m.Level,
                Rank         = m.Rank
            })
            .ToList();

        return new MissionRanksResponse
        {
            MissionSetId = request.MissionSetId,
            Ranks        = rankList
        };
    }

    [CommandHandler]
    public async Task<StartMissionResponse> StartMission(StartMissionRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        logger.LogInformation((int)RequestCommand.MissionRanks,
            "Start mission: [{ServerId}/{ChannelId:00}]: o2ma{MusicId} ({MissionLevel:00})",
            actor.ServerId, Channel.Id, request.MusicId, request.MissionLevel);

        if (!Channel.GetMusicList().TryGetValue(request.MusicId, out var music))
            return new StartMissionResponse { Result = StartMissionResponse.StartMissionResult.InvalidPlanet };

        if (music.MissionLevel != request.MissionLevel)
            return new StartMissionResponse { Result = StartMissionResponse.StartMissionResult.InvalidLevel };

        var gateway     = gatewayOptions.Value;
        var freeMission = gateway.FreeMission ?? gameOptions.Value.FreeMission;
        var useTicket   = gateway.MissionUseTicket;
        var cost        = gateway.MissionCost;

        var user = (await repository.Find(actor.UserId, cancellationToken))!;

        if (!freeMission)
        {
            if (useTicket && user.Ticket < cost)
                return new StartMissionResponse { Result = StartMissionResponse.StartMissionResult.InsufficientTicket };

            if (!useTicket && user.Gem < cost)
                return new StartMissionResponse { Result = StartMissionResponse.StartMissionResult.InsufficientGem };

            if (useTicket)
                user.Ticket -= cost;
            else
                user.Gem -= cost;

            await repository.Update(user, cancellationToken);
            await repository.Commit(cancellationToken);
        }

        actor.Sync(user);
        tracker.Track(Session, request.MissionLevel, request.MusicId, actor.ServerId);

        return new StartMissionResponse
        {
            Result   = StartMissionResponse.StartMissionResult.Success,
            Currency = useTicket ? StartMissionResponse.SpendingType.Ticket : StartMissionResponse.SpendingType.Gem,
            Value    = useTicket ? user.Ticket : user.Gem
        };
    }

    [CommandHandler]
    public async Task<CompleteMissionResponse> CompleteMission(CompleteMissionRequest request, CancellationToken cancellationToken)
    {
        var actor = Session.Actor;
        var state = tracker.Complete(Session);

        logger.LogInformation(
            (int)RequestCommand.CompleteMission,
            "Complete mission: [{ServerId}/{ChannelId:00} - {SetId:00}/{MissionLevel:00}]",
            actor.ServerId, Channel.Id, request.MissionSetId, state.MissionLevel
        );

        float percentage = request.MaxScore > 0
            ? Math.Clamp((float)request.Score / request.MaxScore, 0f, 1f)
            : 0f;

        bool allCombo = request is { Bad: 0, Miss: 0 };
        var rank = RankExtensions.FromPercentage(percentage, allCombo).ToRank();

        // Look up previous best rank
        var existing = actor.CompletedMissionList
            .SingleOrDefault(m => m.GatewayId == actor.ServerId
                               && m.SetId == request.MissionSetId
                               && m.Level == state.MissionLevel);

        var result = CompleteMissionResponse.MissionResult.Success;
        var bestRank = existing?.Rank ?? Rank.None;

        var user = (await repository.Find(actor.UserId, cancellationToken))!;
        if (existing == null)
        {
            user.CompletedMissionList.Add(new CompletedMission
            {
                UserId    = actor.UserId,
                GatewayId = actor.ServerId,
                SetId     = request.MissionSetId,
                Level     = state.MissionLevel,
                Rank      = rank
            });
        }
        else if (rank < existing.Rank)
        {
            existing.Rank = rank;
        }
        else
        {
            result = CompleteMissionResponse.MissionResult.NotRecorded;
        }

        await repository.Update(user, cancellationToken);
        await repository.Commit(cancellationToken);
        actor.Sync(user);

        return new CompleteMissionResponse
        {
            Error         = CompleteMissionResponse.ErrorCode.None,
            Result        = result,
            MissionSetId  = request.MissionSetId,
            MissionLevel  = state.MissionLevel,
            Rank          = rank,
            BestRank      = bestRank,
            Level         = actor.Level,
            RewardGemStar = 0,
            TotalGemStar  = actor.GemStar,
        };
    }
}
