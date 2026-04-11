using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages.Responses;

public class WaitingStateChangedEventData : IMessage
{
    public static Enum Command => EventCommand.WaitingStateChanged;

    [MessageField(order: 0)]
    public byte MemberId { get; init; }

    [MessageField(order: 1)]
    public WaitingState State { get; init; }
}
