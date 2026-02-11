using Amadeus.Messages.Events;
using Microsoft.Extensions.Logging;
using Mozart.Events;
using Mozart.Metadata;
using Mozart.Services;

namespace Amadeus.Events;

public class RoomServiceEventPublisher(ILogger<RoomServiceEventPublisher> logger) : IEventPublisher<RoomService>
{
    public void Monitor(RoomService service)
    {
        service.RoomCreated += OnRoomCreated;
        service.RoomDeleted += OnRoomDeleted;
    }

    private async void OnRoomCreated(object? sender, RoomEventArgs e)
    {
        try
        {
            var room = e.Room;
            await e.Channel.Broadcast(room.Master, new RoomCreatedEventData
            {
                Number        = room.Id,
                Title         = room.Title,
                Mode          = room.Mode,
                HasPassword   = !string.IsNullOrEmpty(room.Password),
                MinLevelLimit = (byte)room.Metadata.MinLevelLimit,
                MaxLevelLimit = (byte)room.Metadata.MaxLevelLimit
            }, CancellationToken.None);

            if (room.Mode == GameMode.Jam)
            {
                room.Channel.GetAlbumList().TryGetValue(room.MusicId, out var album);
                await e.Channel.Broadcast(room.Master, new RoomParameterChangedEventData
                {
                    Number = room.Id,
                    Parameter = new RoomParameterChangedEventData.AlbumParameter
                    {
                        AlbumId = (ushort)room.MusicId,
                        Speed = room.Speed
                    }
                }, CancellationToken.None);

                await e.Channel.Broadcast(room.Master, new RoomAlbumMusicListEventData
                {
                    Number = room.Id,
                    MusicIds = album?.Entries.Select(m => m.Id).ToList() ?? []
                }, CancellationToken.None);
            }
            else
            {
                await e.Channel.Broadcast(room.Master, new RoomParameterChangedEventData
                {
                    Number = room.Id,
                    Parameter = new RoomParameterChangedEventData.MusicParameter
                    {
                        MusicId = (ushort)room.MusicId,
                        Difficulty = room.Difficulty,
                        Speed = room.Speed,
                    }
                }, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [RoomService::OnRoomCreated] event to one or more subscribers");
        }
    }

    private async void OnRoomDeleted(object? sender, RoomEventArgs e)
    {
        try
        {
            await e.Channel.Broadcast(new RoomRemovedEventData
            {
                Number = e.Room.Id
            }, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to broadcast [RoomService::OnRoomDeleted] event to one or more subscribers");
        }
    }
}
