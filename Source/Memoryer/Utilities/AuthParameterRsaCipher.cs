using System.Globalization;
using System.Text;

namespace Memoryer;

public static class AuthParameterRsaCipher
{
    private const uint P = 251;
    private const uint Q = 269;
    private const uint N = P * Q;
    private const uint E = 20891;
    private const uint D = 68711;

    private const uint NoPreimage = uint.MaxValue;

    private static readonly uint[] Encryptable = BuildEncryptLookup();

    public static string GenerateSafeToken(int length, Random? rng = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        if (length == 0)
            return string.Empty;

        const string alphabet = "0123456789";
        rng ??= Random.Shared;

        char[] result = new char[length];
        result[0] = alphabet[rng.Next(alphabet.Length)];

        for (int i = 1; i < length; i++)
        {
            while (true)
            {
                char c = alphabet[rng.Next(alphabet.Length)];
                uint pair = ((uint)result[i - 1] << 8) | c;

                if (Encryptable[pair] == NoPreimage)
                    continue;

                result[i] = c;
                break;
            }
        }

        return new string(result);
    }

    public static string Encrypt(string plaintext)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
        if (bytes.Length % 2 != 0)
        {
            Array.Resize(ref bytes, bytes.Length + 1);
            bytes[^1] = 0x20;
        }

        var sb = new StringBuilder(bytes.Length / 2 * 6);
        for (int i = 0; i < bytes.Length; i += 2)
        {
            uint value = (uint)((bytes[i] << 8) | bytes[i + 1]);
            uint cipher = Encryptable[value];
            if (cipher == NoPreimage)
                cipher = ModExp(value, E, N);

            sb.Append(cipher.ToString("x6"));
        }

        return sb.ToString().ToUpperInvariant();
    }

    private static uint[] BuildEncryptLookup()
    {
        // This is an attempt to fight back the buggy encryption/decryption in the client that often times causing an issue
        // Particularly issue that happen during roundtrip (e.g, encrypted data yield garbles after decryption)
        // This will guarantee that at least full numeric string are reachable, while improving the rest

        uint[] table = new uint[N];
        Array.Fill(table, NoPreimage);

        for (uint c = 0; c < N; c++)
        {
            uint p = ModExp(c, D, N);
            if (table[p] == NoPreimage)
                table[p] = c;
        }

        return table;
    }

    public static string? Decrypt(string ciphertext)
    {
        if (ciphertext.Length % 6 != 0)
            return null;

        int blockCount = ciphertext.Length / 6;
        byte[] output = new byte[blockCount * 2];
        int outIndex = 0;

        for (int i = 0; i < ciphertext.Length; i += 6)
        {
            uint cipher = uint.Parse(ciphertext.AsSpan(i, 6), NumberStyles.HexNumber);
            uint value = ModExp(cipher, D, N);

            string hex = value.ToString("x");
            output[outIndex++] = HexToByte(hex, 0);
            output[outIndex++] = HexToByte(hex, 2);
        }

        return Encoding.UTF8.GetString(output).TrimEnd(' ', '\0');
    }

    private static uint ModExp(uint value, uint exp, uint mod)
    {
        bool completed = false;
        uint acc = 1;

        while (!completed && value != 1)
        {
            uint k = LogCeil(value, mod);
            if (k == 0)
                return 0;

            uint r = exp % k;
            exp = (exp - r) / k;

            uint br = (uint)Math.Pow(value, r);
            acc = (acc * br) % mod;

            if (exp == 0)
                return acc;

            completed = exp == 1;
            value = (uint)(Math.Pow(value, k) % mod);
        }

        return (value * acc) % mod;
    }

    private static uint LogCeil(uint x, uint mod)
    {
        switch (x)
        {
            case 1:
                return 1;
            case 0:
                return uint.MaxValue;
        }

        for (uint y = 1; y <= 4096; y++)
        {
            long v = (long)Math.Pow(x, y);
            if (v == mod)
                return 0;

            if (v > mod)
                return y;
        }
        return uint.MaxValue;
    }

    private static byte HexToByte(string hex, int offset)
    {
        char c1 = offset     < hex.Length ? hex[offset]     : '\0';
        char c2 = offset + 1 < hex.Length ? hex[offset + 1] : '\0';

        if (c1 == '\0')
            return 0;

        int high = HexDigit(c1);
        if (c2 == '\0')
            return (byte)high;

        int low = HexDigit(c2);
        return (byte)((high << 4) | low);
    }

    private static int HexDigit(char c)
    {
        return c switch
        {
            >= '0' and <= '9' => c - '0',
            >= 'A' and <= 'F' => c - 'A' + 10,
            >= 'a' and <= 'f' => c - 'a' + 10,
            _ => 0
        };
    }
}
