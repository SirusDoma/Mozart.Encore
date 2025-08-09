using Microsoft.Extensions.Logging;

using Mozart.Entities;
using Mozart.Messages;
using Mozart.Messages.Events;
using Mozart.Messages.Responses;
using Mozart.Metadata;
using Mozart.Metadata.Room;

namespace Mozart.Events;

public class RoomEventPublisher(ILogger<RoomEventPublisher> logger) : IEventPublisher<Room>
{
    public void Monitor(Room room)
    {
        room.UserJoined            += OnUserJoined;
        room.UserLeft              += OnUserLeft;
        room.UserDisconnected      += OnUserDisconnected;
        room.UserTeamChanged       += OnUserTeamChanged;
        room.UserReadyStateChanged += OnUserReadyStateChanged;

        room.TitleChanged += OnTitleChanged;
        room.MusicChanged += OnMusicChanged;
        room.ArenaChanged += OnArenaChanged;
        room.StateChanged += OnStateChanged;
        room.SlotChanged  += OnSlotChanged;
    }

    private async void OnUserJoined(object? sender, RoomUserJoinedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));
            await room.Broadcast(sender: e.Member.Session, new UserJoinWaitingEventData
            {
                MemberId   = (byte)e.MemberId,
                Nickname   = e.Member.Actor.Nickname,
                Level      = e.Member.Actor.Level,
                Gender     = e.Member.Actor.Gender,
                Team       = e.Member.Team,
                Ready      = e.Member.IsReady,
                Equipments = e.Member.Actor.Equipments,
                MusicIds   = e.Member.Actor.MusicIds
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast [Room::OnUserJoined] event to one or more subscribers");
        }
    }

    private async void OnUserLeft(object? sender, RoomUserLeftEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(sender: e.Member.Session, new UserLeaveWaitingEventData
            {
                MemberId              = (byte)e.MemberId,
                NewRoomMasterMemberId = (byte)e.RoomMasterMemberId,
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast [Room::OnUserLeft] event to one or more subscribers");
        }
    }

    private void OnUserDisconnected(object? sender, RoomUserLeftEventArgs e)
    {
        try
        {
            logger.LogWarning("User [{User}] is failed to ready within timeout limit", e.Member.Actor.Nickname);
            var session = e.Member.Session;

            if (session.Room != null)
                session.Exit(session.Room);

            if (session.Channel != null)
                session.Exit(session.Channel);

            session.Terminate();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast [Room::OnUserDisconnected] event to one or more subscribers");
        }
    }

    private async void OnUserTeamChanged(object? sender, RoomUserTeamChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(new MemberTeamChangedEventData
            {
                MemberId = (byte)e.MemberId,
                Team     = e.Team
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnUserTeamChanged] event to one or more subscribers");
        }
    }

    private async void OnUserReadyStateChanged(object? sender, RoomUserReadyStateChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(new MemberReadyStateChangedEventData
            {
                MemberId = (byte)e.MemberId,
                Ready    = e.Ready
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnUserReadyStateChanged] event to one or more subscribers");
        }
    }

    private async void OnTitleChanged(object? sender, RoomTitleChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Master.Channel!.Broadcast(session => !room.IsMember(session), new RoomTitleChangedEventData
            {
                Number = room.Id,
                Title  = room.Title
            }, CancellationToken.None);

            await room.Broadcast(sender: room.Master, new WaitingRoomTitleEventData
            {
                Title  = room.Title
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnTitleChanged] event to one or more subscribers");
        }
    }

    private async void OnMusicChanged(object? sender, RoomMusicChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Master.Channel!.Broadcast(session => !room.IsMember(session), new RoomMusicChangedEventData
            {
                Number     = room.Id,
                MusicId    = room.MusicId,
                Difficulty = room.Difficulty,
                Speed      = room.Speed,
            }, CancellationToken.None);

            await room.Broadcast(sender: room.Master, new WaitingMusicChangedEventData
            {
                MusicId    = room.MusicId,
                Difficulty = room.Difficulty,
                Speed      = room.Speed,

            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnMusicChanged] event to one or more subscribers");
        }
    }

    private async void OnArenaChanged(object? sender, RoomArenaChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(sender: room.Master, new WaitingArenaChangedEventData
            {
                Arena      = room.Arena,
                RandomSeed = room.ArenaRandomSeed
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnArenaChanged] event to one or more subscribers");
        }
    }

    private async void OnStateChanged(object? sender, RoomStateChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Master.Channel!.Broadcast(session => !room.IsMember(session), new RoomStateChangedEventData
            {
                Number = room.Id,
                State  = room.State
            }, CancellationToken.None);

            if (e.PreviousState == RoomState.Waiting)
            {
                await room.Broadcast(sender: room.Master, new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.Success
                }, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnStateChanged] event to one or more subscribers");
        }
    }

    private async void OnSlotChanged(object? sender, RoomSlotChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));
        
            switch (e.ActionType)
            {
                case RoomSlotActionType.PlayerKicked when e.PreviousSlot is Room.MemberSlot member:
                    await member.Session.WriteMessage(new KickEventData(), CancellationToken.None);
                    break;
            }

            await room.Broadcast(new RoomSlotUpdateEventData
            {
                Index = (byte)e.SlotId,
                Type  = e.ActionType
            }, CancellationToken.None);

            await room.Master.Channel!.Broadcast(session => !room.IsMember(session), new RoomUserCountChangedEventData
            {
                Number    = room.Id,
                Capacity  = (byte)e.Capacity,
                UserCount = (byte)e.UserCount
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast [Room::OnSlotChanged] event to one or more subscribers");
        }
    }

}