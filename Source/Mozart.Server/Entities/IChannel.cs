using Mozart.Services;
using Mozart.Sessions;

namespace Mozart.Entities;


public interface IChannel : IBroadcastable
{
    int Id         { get; }
    int Capacity   { get; }
    int UserCount  { get; }
    float GemRates { get; }
    float ExpRates { get; }

    string MusicListFileName { get; }
    string ItemDataFileName { get; }

    void Register(Session session);

    void Remove(Session session);
}
