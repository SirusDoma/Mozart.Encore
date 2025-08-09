using Encore.Messaging;
using Mozart.Metadata;

namespace Mozart.Messages.Responses;

public class WaitingArenaChangedEventData : IMessage
{
    public static Enum Command => EventCommand.RoomArenaChanged;

    public Arena Arena { get; init; } = Arena.Random;

    public byte RandomSeed { get; init; }

    [MessageField(order: 0)]
    private byte Prefix => Arena == Arena.Random ? RandomSeed : (byte)Arena;

    [MessageField(order: 1)]
    private ushort Padding => 0;

    [MessageField(order: 2)]
    private byte Suffix => Arena == Arena.Random ? (byte)Arena.Random : (byte)0;
}