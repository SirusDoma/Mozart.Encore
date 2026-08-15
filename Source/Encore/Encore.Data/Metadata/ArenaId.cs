namespace Encore.Metadata;

public static class ArenaId
{
    public const byte RandomFlag = 0x80;

    public static int Random(byte seed) => unchecked((int)0x8000_0000) | seed;

    public static bool IsRandom(int value) => value >>> 24 == RandomFlag;

    public static int Id(int value) => value & 0xFFFF;

    public static byte Seed(int value) => IsRandom(value) ? (byte)(value & 0xFF) : (byte)0;
}
