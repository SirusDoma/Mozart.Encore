using Encore.Messaging;

namespace Identity.Messages.Responses;

public class GiftFreePassResponse : IMessage
{
    public static Enum Command => ResponseCommand.GiftFreePass;

    [MessageField(order: 1)]
    public int ErrorCode { get; init; }

    public TimeSpan ExtensionPeriod { get; init; }

    [MessageField(order: 2)]
    private int ExtensionDays => Math.Max((int)Math.Ceiling(ExtensionPeriod.TotalDays), 0);
}
