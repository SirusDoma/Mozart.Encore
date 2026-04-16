using Encore.Messaging;

namespace Identity.Messages.Requests;

public class ExitWaitingRequest : IMessage
{
    public static Enum Command => RequestCommand.ExitWaiting;

    public enum ExitType : ushort
    {
        Normal    = 0x0000, // 0
        Penalized = 0x0001  // 1
    }

    [MessageField(order: 0)]
    public ExitType Type { get; init; }
}
