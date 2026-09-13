using Encore.Data.Entities;
using Encore.Messaging;

namespace Encore.Messages.Requests;

public class CreateSessionRequest : IMessage
{
    public static Enum Command => GatewayCommand.CreateSession;

    [MessageField(order: 0)]
    public required int UserId { get; init; }

    [MessageField(order: 1)]
    public required ushort GatewayId { get; init; }

    [MessageField(order: 2)]
    public required ushort ChannelId { get; init; }

    [MessageField(order: 3)]
    public FreePassType FreePassType { get; init; }

    [MessageField(order: 4)]
    public int Ranking { get; init; }
}
