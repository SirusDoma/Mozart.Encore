using Encore.Messaging;
using Mozart.Metadata;

namespace Amadeus.Messages;

public class RoomArenaMessage : SubMessage
{
    public RoomArenaMessage()
    {
    }

    public RoomArenaMessage(Arena arena, byte randomSeed = 0)
    {
        if (arena == Arena.Random)
        {
            Suffix = (byte)arena;
            Prefix = randomSeed;
        }
        else
        {
            Prefix = (byte)arena;
            Suffix = 0;
        }
    }

    [MessageField(order: 0)]
    private byte Prefix { get; init; }

    [MessageField(order: 1)]
    private ushort Padding { get; init; }

    [MessageField(order: 2)]
    private byte Suffix { get; init; }

    public Arena Arena => Suffix == (byte)Arena.Random ? Arena.Random : (Arena)Prefix;

    public byte RandomSeed => Suffix == (byte)Arena.Random ? Prefix : (byte)0;
}
