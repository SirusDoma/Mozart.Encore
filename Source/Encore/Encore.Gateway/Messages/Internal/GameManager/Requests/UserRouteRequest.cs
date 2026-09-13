using Encore.Messaging;

namespace Encore.Messages.Requests;

public class UserRouteRequest : IMessage
{
    public static Enum Command => GatewayCommand.UserRoute;

    [MessageField(order: 0)]
    public uint SessionId { get; init; }

    [MessageField(order: 1)]
    public int UserId { get; init; }
}
