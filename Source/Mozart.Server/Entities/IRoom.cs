using Mozart.Metadata;
using Mozart.Metadata.Room;
using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Entities;

public interface IRoom : IBroadcastable
{
    int Id { get; }
    IChannel Channel { get; }
    RoomState State { get; }
    RoomMetadata Metadata { get; }
    int Capacity { get; }
    int UserCount { get; }
    string Title { get; set; }
    string Password { get; }
    int MusicId { get; set; }
    Difficulty Difficulty { get; set; }
    GameSpeed Speed { get; set; }
    Arena Arena { get; set; }
    byte ArenaRandomSeed { get; set; }
    Session Master { get; }
    IReadOnlyList<Room.ISlot> Slots { get; }
    IScoreTracker ScoreTracker { get; }

    void Register(Session session);
    void Remove(Session session);
    void SaveMetadataChanges();

    void UpdateReadyState(Session session);
    void UpdateTeam(Session session, RoomTeam team);
    void UpdateSlot(Session session, int slotId);

    void StartGame();
    void CompleteGame();

    void Disconnect(Session session);
}
