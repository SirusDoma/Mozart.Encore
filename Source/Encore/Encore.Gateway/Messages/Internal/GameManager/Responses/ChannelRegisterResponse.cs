using Encore.Messaging;

namespace Encore.Messages.Responses;

public class ChannelRegisterResponse : IMessage
{
    public static Enum Command => GameManagerCommand.ChannelRegister;

    [MessageField<MessageFieldCodec<int>>(order: 0)]
    public bool Invalid { get; init; }

    [MessageField(order: 1)]
    public ushort GatewayId { get; init; }

    [MessageField(order: 2)]
    public ushort PlanetKind { get; init; }
}
