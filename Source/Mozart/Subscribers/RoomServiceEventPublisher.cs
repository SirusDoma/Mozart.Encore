using Microsoft.Extensions.Logging;
using Mozart.Messages.Events;
using Mozart.Services;

namespace Mozart.Events;

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
                Mode          = room.Metadata.Mode,
                HasPassword   = !string.IsNullOrEmpty(room.Password),
                MinLevelLimit = (byte)room.Metadata.MinLevelLimit,
                MaxLevelLimit = (byte)room.Metadata.MaxLevelLimit
            }, CancellationToken.None);

            await e.Channel.Broadcast(room.Master, new RoomMusicChangedEventData
            {
                Number     = room.Id,
                MusicId    = room.MusicId,
                Difficulty = room.Difficulty,
                Speed      = room.Speed,
            }, CancellationToken.None);
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