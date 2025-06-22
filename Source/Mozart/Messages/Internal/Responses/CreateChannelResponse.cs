using Encore.Messaging;

namespace Mozart.Internal.Requests;

public class CreateChannelResponse : IMessage
{
    public static Enum Command => GatewayCommand.CreateChannel;

    [MessageField(order: 0)]
    public bool Success { get; init; }
}