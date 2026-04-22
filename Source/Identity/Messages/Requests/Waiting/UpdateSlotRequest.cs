using Encore.Messaging;

namespace Memoryer.Messages.Requests;

public class UpdateSlotRequest : IMessage
{
    public static Enum Command => RequestCommand.UpdateSlot;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }
}
