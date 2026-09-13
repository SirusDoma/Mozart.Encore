using Encore.Messaging;

namespace Encore.Messages.Requests;

public class GatewayRegisterRequest : IMessage
{
    public static Enum Command => GatewayCommand.GatewayRegister;

    [StringMessageField(order: 0)]
    public string GatewayName { get; init; } = string.Empty;
}
