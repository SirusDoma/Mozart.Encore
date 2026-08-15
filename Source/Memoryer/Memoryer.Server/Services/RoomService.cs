using Encore.Events;
using Encore.Metadata;
using Encore.Server.Sessions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mozart.Entities;
using Mozart.Metadata;
using Mozart.Metadata.Room;
using Mozart.Options;
using Mozart.Sessions;

namespace Mozart.Services;

public interface IRoomService
{
    Room CreateRoom(Session session, string title, KeyMode keyMode, GameMode gameMode, string password,
        int minLevelLimit, int maxLevelLimit, bool premium, int type);

    Room DeleteRoom(Encore.Entities.IChannel channel, int id);

    Room GetRoom(Encore.Entities.IChannel channel, int id);

    IReadOnlyList<Room> GetRooms(Encore.Entities.IChannel channel);
}

public class RoomService : Encore.Services.RoomService<Room>, IRoomService
{
    private readonly IOptions<GameOptions> _options;

    public RoomService(IEventPublisher<RoomService> publisher, IOptions<GameOptions> options,
        ILogger<RoomService> logger) : base(logger)
    {
        publisher.Monitor(this);
        _options = options;
    }

    public Room CreateRoom(Session session, string title, KeyMode keyMode, GameMode gameMode, string password,
        int minLevelLimit, int maxLevelLimit, bool premium, int type)
    {
        return AddRoom(session, _ =>
        {
            int musicId = session.GetAuthorizedToken<Actor>().InstalledMusicIds.FirstOrDefault((ushort)0) & 0x0FFF;

            return id => new Room(this, session, new RoomMetadata
            {
                Id              = id,
                Title           = title,
                KeyMode         = keyMode,
                GameMode        = gameMode,
                MusicId         = musicId,
                Difficulty      = Difficulty.EX,
                Speed           = GameSpeed.X10,
                MinLevelLimit   = minLevelLimit,
                MaxLevelLimit   = maxLevelLimit,
                Arena           = ArenaId.Random((byte)Random.Shared.Next(0, 0x24)),
                Password        = password,
                State           = RoomState.Waiting,
                Premium         = premium,
                Type            = type
            }, _options.Value);
        });
    }
}
