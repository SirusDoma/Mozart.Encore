using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mozart.Services;

namespace Amadeus;

public static class WorkerExtensions
{
    public static void ValidateMetadata(this IHostedService service, IChannelService channelService,
        IMetadataResolver resolver, ILogger logger)
    {
        try
        {
            foreach (var channel in channelService.GetChannels())
                _ = resolver.GetMusicList(channel);

            try
            {
                foreach (var channel in channelService.GetChannels())
                    _ = resolver.GetAlbumList(channel);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to validate album list file");
            }

            foreach (var channel in channelService.GetChannels())
                _ = resolver.GetItemData(channel);
        }
        catch (Exception)
        {
            logger.LogError("Failed to validate metadata files");
            throw;
        }
    }
}
