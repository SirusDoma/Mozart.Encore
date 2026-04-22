using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mozart.Services;

namespace Memoryer;

public static class WorkerExtensions
{
    public static void ValidateMetadata(this IHostedService service, IChannelService channelService,
        IMetadataResolver resolver, ILogger logger)
    {
        foreach (var channel in channelService.GetChannels())
        {
            try
            {
                _ = resolver.GetMusicList(channel);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to validate music list file");
            }
        }

        foreach (var channel in channelService.GetChannels())
        {
            try
            {
                _ = resolver.GetAlbumList(channel);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to validate album list file");
            }
        }

        foreach (var channel in channelService.GetChannels())
        {
            try
            {
                _ = resolver.GetItemData(channel);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to validate item data file");
            }
        }
    }
}
