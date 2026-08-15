using System.Globalization;
using System.Numerics;
using System.Text;

namespace Identity;

public static class AuthParameterRsaCipher
{
    private const int P = 251;
    private const int Q = 269;
    private const int N = P * Q;
    private const int E = 54391;
    private const int D = 68711;

    public static string Encrypt(string plaintext)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
        if (bytes.Length % 2 != 0)
        {
            Array.Resize(ref bytes, bytes.Length + 1);
            bytes[^1] = 0;
        }

        var sb = new StringBuilder(bytes.Length / 2 * 6);
        for (int i = 0; i < bytes.Length; i += 2)
        {
            int value = (bytes[i] << 8) | bytes[i + 1];
            int cipher = (int)BigInteger.ModPow(value, E, N);
            sb.Append(cipher.ToString("X6"));
        }

        return sb.ToString();
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
            int cipher = int.Parse(ciphertext.Substring(i, 6), NumberStyles.HexNumber);
            int value = (int)BigInteger.ModPow(cipher, D, N);

            string hex = value.ToString("x");
            output[outIndex++] = hex.Length >= 2 ? Convert.ToByte(hex[..2], 16) : Convert.ToByte(hex[..1], 16);
            output[outIndex++] = hex.Length >= 4 ? Convert.ToByte(hex[2..4], 16) : hex.Length == 3 ? Convert.ToByte(hex[2..3], 16) : (byte)0;
        }

        return Encoding.UTF8.GetString(output).TrimEnd('\0');
    }
}
