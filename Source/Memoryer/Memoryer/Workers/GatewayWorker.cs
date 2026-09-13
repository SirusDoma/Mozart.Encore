using Memoryer;

namespace Encore.Workers;

public partial class GatewayWorker
{
    private partial Version Version => Program.Version;
    private partial Version NetworkVersion => Program.NetworkVersion;
    private partial string DatabaseDriverName => dbOptions.Value.Driver.GetPrintableName();
}
