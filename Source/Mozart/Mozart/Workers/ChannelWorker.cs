using Mozart;

namespace Encore.Workers;

public partial class ChannelWorker
{
    private partial Version Version => Program.Version;
    private partial Version NetworkVersion => Program.NetworkVersion;
    private partial string DatabaseDriverName => dbOptions.Value.Driver.GetPrintableName();

    private partial void ValidateChannelMetadata() => this.ValidateMetadata(channelService, resolver, logger);
}
