using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

using Mozart.Entities;
using Mozart.Messages;
using Mozart.Messages.Events;
using Mozart.Messages.Requests;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IRoomService
{
    Task<Room> CreateRoom(Session session, CreateRoomRequest request, CancellationToken cancellationToken);

    Task<Room> DeleteRoom(IChannel channel, int id, CancellationToken cancellationToken);

    Room GetRoom(IChannel channel, int id);

    IReadOnlyList<Room> GetRooms(IChannel channel);
}

public class RoomService(ILogger<RoomService> logger) : Broadcastable, IRoomService
{
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Room>> _rooms = [];

    public override IReadOnlyList<Session> Sessions =>
        _rooms.Values.SelectMany(e => e.Values.SelectMany(r => r.Sessions)).ToList();

    public async Task<Room> CreateRoom(Session session, CreateRoomRequest request, CancellationToken cancellationToken)
    {
        if (session.Room != null)
            throw new ArgumentOutOfRangeException(nameof(session));

        if (session.Channel == null)
            throw new ArgumentOutOfRangeException(nameof(session));

        var channel = session.Channel;
        _rooms.TryAdd(channel.Id, []);

        var rooms = _rooms[channel.Id];
        if (rooms.Count >= channel.Capacity)
            throw new InvalidOperationException("Channel is full");

        for (int i = 0; i < channel.Capacity; i++)
        {
            var room = new Room(this, session, new RoomMetadata
            {
                Id              = i,
                Title           = request.Title,
                Mode            = request.Mode,
                MusicId         = session.GetAuthorizedToken<Actor>().MusicIds[0],
                Difficulty      = Difficulty.EX,
                Speed           = GameSpeed.X10,
                MinLevelLimit   = request.MinLevelLimit,
                MaxLevelLimit   = request.MaxLevelLimit,
                Arena           = Arena.Random,
                ArenaRandomSeed = (byte)Random.Shared.Next(0, (int)Arena.AWhaleOfAqua),
                Password        = request.HasPassword ? request.Password : string.Empty,
                State           = RoomState.Waiting
            });

            if (rooms.TryAdd(i, room))
            {
                room.SessionDisconnected += OnRoomSessionDisconnected;

                await session.Register(room, cancellationToken);
                await session.Channel!.Broadcast(session, new RoomCreatedEventData
                {
                    Number        = room.Id,
                    Title         = room.Title,
                    Mode          = room.Metadata.Mode,
                    HasPassword   = request.HasPassword,
                    MinLevelLimit = (byte)room.Metadata.MinLevelLimit,
                    MaxLevelLimit = (byte)room.Metadata.MaxLevelLimit
                }, cancellationToken);

                await session.Channel!.Broadcast(session, new RoomMusicChangedEventData
                {
                    Number     = room.Id,
                    MusicId    = room.MusicId,
                    Difficulty = room.Difficulty,
                    Speed      = room.Speed,

                }, cancellationToken);

                return room;
            }
        }

        throw new InvalidOperationException("Channel is full");
    }

    public async Task<Room> DeleteRoom(IChannel channel, int id, CancellationToken cancellationToken)
    {
        if (!_rooms.TryGetValue(channel.Id, out var rooms))
            throw new ArgumentOutOfRangeException(nameof(channel));

        if (!rooms.TryRemove(id, out var room))
            throw new ArgumentOutOfRangeException(nameof(id));

        await channel.Broadcast(new RoomRemovedEventData
        {
            Number = room.Id
        }, cancellationToken);

        return room;
    }

    public Room GetRoom(IChannel channel, int id)
    {
        if (!_rooms.TryGetValue(channel.Id, out var rooms))
            throw new ArgumentOutOfRangeException(nameof(channel));

        if (!rooms.TryGetValue(id, out var room))
            throw new ArgumentOutOfRangeException(nameof(id));

        return room;
    }

    public IReadOnlyList<Room> GetRooms(IChannel channel)
    {
        _rooms.TryAdd(channel.Id, []);
        return _rooms[channel.Id].Values.ToList();
    }

    protected override IEnumerable<Session> GetSessionsByContext<TContext>(TContext ctx)
    {
        return [];
    }

    private void OnRoomSessionDisconnected(object? sender, Encore.Sessions.SessionEventArgs e)
    {
        logger.LogWarning("Session [{User}] removed from the room due to connection lost",
            e.Session.Socket.RemoteEndPoint);
    }
}