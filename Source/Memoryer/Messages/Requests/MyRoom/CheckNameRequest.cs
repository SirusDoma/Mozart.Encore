using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class CheckNameRequest : IMessage
{
    public static Enum Command => RequestCommand.CheckName;

    [StringMessageField(order: 0)]
    public required string Name { get; init; }
}
