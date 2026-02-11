using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Entities;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Metadata.Room;
using Mozart.Options;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IRoomService
{
    Room CreateRoom(Session session, string title, GameMode mode, string password,
        int minLevelLimit, int maxLevelLimit);

    Room DeleteRoom(IChannel channel, int id);

    Room GetRoom(IChannel channel, int id);

    IReadOnlyList<Room> GetRooms(IChannel channel);
}

public class RoomEventArgs : EventArgs
{
    public required IChannel Channel { get; init; }
    public required IRoom Room { get; init; }
}

public class RoomService : Broadcastable, IRoomService
{
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, Room>> _rooms = [];
    private readonly IOptions<GameOptions> _options;
    private readonly ILogger<RoomService> _logger;

    public event EventHandler<RoomEventArgs>? RoomCreated;
    public event EventHandler<RoomEventArgs>? RoomDeleted;

    public RoomService(IEventPublisher<RoomService> publisher, IOptions<GameOptions> options,
        ILogger<RoomService> logger)
    {
        publisher.Monitor(this);

        _options = options;
        _logger  = logger;
    }

    public override IReadOnlyList<Session> Sessions =>
        _rooms.Values.SelectMany(e => e.Values.SelectMany(r => r.Sessions)).ToList();

    public Room CreateRoom(Session session, string title, GameMode mode, string password,
        int minLevelLimit, int maxLevelLimit)
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

        int musicId = session.GetAuthorizedToken<Actor>().InstalledMusicIds.FirstOrDefault((ushort)0);
        if (mode == GameMode.Jam)
            musicId = channel.GetAlbumList().FirstOrDefault().Value.AlbumId;

        for (int i = 0; i < channel.Capacity; i++)
        {
            var room = new Room(this, session, new RoomMetadata
            {
                Id              = i,
                Title           = title,
                Mode            = mode,
                MusicId         = musicId,
                Difficulty      = Difficulty.EX,
                Speed           = GameSpeed.X10,
                MinLevelLimit   = minLevelLimit,
                MaxLevelLimit   = maxLevelLimit,
                Arena           = Arena.Random,
                ArenaRandomSeed = (byte)Random.Shared.Next(0, (int)Arena.AWhaleOfAqua),
                Password        = password,
                State           = RoomState.Waiting
            }, _options.Value.MusicLoadTimeout > 0 ? TimeSpan.FromSeconds(_options.Value.MusicLoadTimeout) : null);

            if (rooms.TryAdd(i, room))
            {
                session.Register(room);
                room.SessionDisconnected += OnRoomSessionDisconnected;

                RoomCreated?.Invoke(this, new RoomEventArgs
                {
                    Channel = session.Channel,
                    Room    = room
                });
                return room;
            }
        }

        throw new InvalidOperationException("Channel is full");
    }

    public Room DeleteRoom(IChannel channel, int id)
    {
        if (!_rooms.TryGetValue(channel.Id, out var rooms))
            throw new ArgumentOutOfRangeException(nameof(channel));

        if (!rooms.TryRemove(id, out var room))
            throw new ArgumentOutOfRangeException(nameof(id));

        RoomDeleted?.Invoke(this, new RoomEventArgs
        {
            Channel = channel,
            Room    = room
        });

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

    public override void Invalidate()
    {
        foreach (var session in Sessions)
        {
            if (session.Connected)
                continue;

            if (session.Channel != null)
                session.Exit(session.Channel);
            else if (session.Room != null)
                session.Exit(session.Room);
        }
    }

    private void OnRoomSessionDisconnected(object? sender, Encore.Sessions.SessionEventArgs e)
    {
        _logger.LogWarning("Session [{User}] removed from the room due to connection lost",
            e.Session.Socket.RemoteEndPoint);
    }
}