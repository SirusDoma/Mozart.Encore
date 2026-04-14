using CrossTime.Messages.Events;
using CrossTime.Messages.Responses;
using Microsoft.Extensions.Logging;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Metadata.Room;

namespace CrossTime.Events;

public class RoomEventPublisher(ILogger<RoomEventPublisher> logger) : IEventPublisher<Room>
{
    public void Monitor(Room room)
    {
        room.UserJoined              += OnUserJoined;
        room.UserLeft                += OnUserLeft;
        room.UserDisconnected        += OnUserDisconnected;
        room.UserTeamChanged         += OnUserTeamChanged;
        room.UserMusicStateChanged += OnUserMusicStateChanged;
        room.UserReadyStateChanged   += OnUserReadyStateChanged;

        room.MusicChanged      += OnMusicChanged;
        room.AlbumChanged      += OnAlbumChanged;
        room.ArenaChanged      += OnArenaChanged;
        room.StateChanged      += OnStateChanged;
        room.SlotChanged       += OnSlotChanged;
        room.SkillChanged      += OnSkillChanged;
        room.ModeChanged       += OnModeChanged;
        room.TeamToggleChanged += OnTeamToggleChanged;
    }

    private async void OnUserJoined(object? sender, RoomUserJoinedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));
            await room.Broadcast(sender: e.Member.Session, new UserJoinWaitingEventData
            {
                MemberId     = (byte)e.MemberId,
                Nickname     = e.Member.Actor.Nickname,
                Level        = e.Member.Actor.Level,
                Gender       = e.Member.Actor.Gender,
                Gem          = e.Member.Actor.Gem,
                Team         = e.Member.Team,
                Ready        = e.Member.IsReady,
                MusicState   = e.Member.MusicState,
                Equipments   = e.Member.Actor.Equipments,
                MusicIds     = e.Member.Actor.InstalledMusicIds,
                CashPoint    = e.Member.Actor.CashPoint
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

    private async void OnUserMusicStateChanged(object? sender, RoomUserMusicStateChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(new MusicStateChangedEventData
            {
                MemberId = (byte)e.MemberId,
                State    = e.State
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnUserMusicStateChanged] event to one or more subscribers");
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

    private async void OnMusicChanged(object? sender, RoomMusicChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(sender: room.Master, new WaitingMusicChangedEventData
            {
                MusicId      = (ushort)room.MusicId,
                MissionLevel = (byte)room.MissionLevel,
            }, CancellationToken.None);

            await room.Channel!.Broadcast(session => !room.IsMember(session), new RoomParameterChangedEventData
            {
                Number = room.Id,
                Parameter = new RoomParameterChangedEventData.MusicParameter
                {
                    MusicId    = (ushort)room.MusicId,
                    Difficulty = room.Difficulty,
                    Speed      = room.Speed,
                }
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnMusicChanged] event to one or more subscribers");
        }
    }

    private async void OnAlbumChanged(object? sender, RoomAlbumChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(sender: room.Master, new WaitingAlbumChangedEventData
            {
                AlbumId = e.AlbumId,
                Speed = e.Speed
            }, CancellationToken.None);

            await room.Channel!.Broadcast(session => !room.IsMember(session), new RoomParameterChangedEventData
            {
                Number = room.Id,
                Parameter = new RoomParameterChangedEventData.AlbumParameter
                {
                    AlbumId = (ushort)e.AlbumId,
                    Speed = e.Speed
                }
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [Room::OnAlbumChanged] event to one or more subscribers");
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

            if (e.PreviousState == RoomState.Waiting)
            {
                await room.Broadcast(sender: room.Master, new StartGameEventData
                {
                    Result = StartGameEventData.StartResult.Success,
                    SkillsSeed = room.SkillsSeed
                }, CancellationToken.None);
            }

            await room.Channel!.Broadcast(session => !room.IsMember(session), new RoomStateChangedEventData
            {
                Number = room.Id,
                State  = room.State
            }, CancellationToken.None);
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

            await room.Channel!.Broadcast(session => !room.IsMember(session), new RoomUserCountChangedEventData
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

    private async void OnSkillChanged(object? sender, RoomSkillChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            await room.Broadcast(sender: room.Master, new WaitingSkillChangedEventData
            {
                Skills = e.Skills
            }, CancellationToken.None);

            await room.Channel!.Broadcast(session => !room.IsMember(session), new RoomSkillChangedEventData
            {
                Number = room.Id,
                Skills = e.Skills
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast [Room::OnSkillChanged] event to one or more subscribers");
        }
    }

    private async void OnModeChanged(object? sender, RoomModeChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));
            await room.Broadcast(sender: room.Master, new WaitingModeChangedEventData
            {
                Number       = room.Id,
                Title        = e.Title,
                Mode         = e.Mode,
                HasPassword  = !string.IsNullOrEmpty(e.Password),
                Password     = e.Password,
                TeamDisabled = !e.TeamEnabled
            }, CancellationToken.None);

            await room.Channel!.Broadcast(session => !room.IsMember(session), new RoomModeChangedEventData
            {
                Number       = room.Id,
                Title        = e.Title,
                Mode         = e.Mode,
                HasPassword  = !string.IsNullOrEmpty(e.Password),
                TeamDisabled = !e.TeamEnabled
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast [Room::OnModeChanged] event to one or more subscribers");
        }
    }

    private async void OnTeamToggleChanged(object? sender, RoomTeamToggleChangedEventArgs e)
    {
        try
        {
            var room = sender as Room ?? throw new ArgumentException(null, nameof(sender));

            var teams = new List<WaitingTeamToggleChangedEventData.MemberTeamInfo>();
            for (int id = 0; id < room.Slots.Count; id++)
            {
                var member = room.Slots[id] as Room.MemberSlot;
                if (member == null)
                    continue;

                teams.Add(new WaitingTeamToggleChangedEventData.MemberTeamInfo
                {
                    MemberId = (byte)id,
                    Team    = member.Team
                });
            }

            await room.Broadcast(new WaitingTeamToggleChangedEventData
            {
                Disabled = !e.Enabled,
                Teams    = teams
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast [Room::OnTeamToggleChanged] event to one or more subscribers");
        }
    }
}
