using Encore.Metadata;
using Encore.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mozart;

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
