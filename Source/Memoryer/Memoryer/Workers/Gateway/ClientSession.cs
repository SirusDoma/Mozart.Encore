using Memoryer;

namespace Encore.Workers.Gateway;

public partial class ClientSession
{
    private static partial bool IsGatewayCommand(ushort command)
    {
        return command is (ushort)RequestCommand.GetChannelList
            or (ushort)RequestCommand.ChannelLogin
            or (ushort)RequestCommand.Terminate;
    }
}
