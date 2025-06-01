using Encore.Messaging;
using Encore.Server;
using Encore.Sessions;

namespace Mozart;

[Authorize]
public class WaitingController(Session session, IMessageCodec codec) : CommandController(session)
{
    [CommandHandler]
    public Task<WaitingMusicChangedEventData> SetRoomMusic(SetRoomMusicRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new WaitingMusicChangedEventData
        {
            MusicId    = request.MusicId,
            Difficulty = request.Difficulty,
            Speed      = request.Speed
        });
    }

    [CommandHandler]
    public Task<SetRoomTitleResponse> SetRoomTitle(SetRoomTitleRequest request, CancellationToken cancellationToken)
    {
        // Broadcast: RoomTitleChangedEventData
        return Task.FromResult(new SetRoomTitleResponse
        {
            Title = request.Title[..Math.Min(21, request.Title.Length)]
        });
    }

    [CommandHandler]
    public Task<PlayerTeamChangedEventData> SetRoomPlayerTeam(SetTeamRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new PlayerTeamChangedEventData
        {
            MemberId = 0,
            Team = request.Team
        });
    }

    [CommandHandler]
    public Task<RoomArenaChangedEventData> SetRoomArena(SetRoomArenaRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RoomArenaChangedEventData
        {
            Arena      = request.Payload.Arena,
            RandomSeed = request.Payload.RandomSeed
        });
    }

    [CommandHandler]
    public Task SetPlayerInstrument(SetInstrumentRequest request, CancellationToken cancellationToken)
    {
        // Do not reply to this command:
        // The game was supposed to send "InstrumentId" but instead, it always sends us 0
        // return Task.FromResult(new PlayerInstrumentChangedEventData()
        // {
        //     MemberId     = 0,
        //     InstrumentId = request.InstrumentId
        // });

        return Task.CompletedTask;
    }

    [CommandHandler]
    public Task<RoomSlotUpdateEventData> KickPlayer(KickRequest request, CancellationToken cancellationToken)
    {
        // Send: RoomKickEventData
        return Task.FromResult(new RoomSlotUpdateEventData
        {
            Index = request.MemberId,
            Type  = RoomSlotUpdateEventData.EventType.PlayerKicked
        });
    }

    [CommandHandler(RequestCommand.Ready)]
    public Task<PlayerReadyStateChangedEventData> Ready(CancellationToken cancellationToken)
    {
        return Task.FromResult(new PlayerReadyStateChangedEventData
        {
            MemberId = 0,
            Ready    = true
        });
    }

    [CommandHandler(RequestCommand.Ready)]
    public Task<StartGameEventData> AutoStartGame(CancellationToken cancellationToken)
    {
        return Task.FromResult(new StartGameEventData
        {
            Result = StartGameEventData.StartResult.Success
        });
    }

    [CommandHandler(RequestCommand.StartGame)]
    public Task<StartGameEventData> StartGame(CancellationToken cancellationToken)
    {
        return Task.FromResult(new StartGameEventData
        {
            Result = StartGameEventData.StartResult.Success
        });
    }

    [CommandHandler(RequestCommand.ConfirmMusicLoaded)]
    public async Task ConfirmMusicLoaded(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 8; i++)
        {
            await Session.WriteFrameAsync(codec.Encode(new MusicLoadedEventData
            {
                MemberId = (byte)i
            }), cancellationToken);
        }
    }

    [CommandHandler(RequestCommand.ExitWaiting)]
    public Task<ExitWaitingResponse> ExitRoom(CancellationToken cancellationToken)
    {
        return Task.FromResult(new ExitWaitingResponse
        {
            Failed = false
        });
    }
}