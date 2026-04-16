using System.Text;

namespace Identity;

public class AuthParameters
{
    public const int FieldCount = 19;

    public string FtpAddresses { get; init; } = "127.0.0.1|127.0.0.1";
    public string FtpPort { get; init; } = "21";
    public string FtpPath1 { get; init; } = "O2Jam";
    public string FtpPath2 { get; init; } = "O2Jam";
    public string GameVersion { get; init; } = "8.05";
    public string UserIndexId { get; init; } = "";
    public string Username { get; init; } = "";
    public string Password { get; init; } = "1234567890";
    public string Level { get; init; } = "0";
    public string Gender { get; init; } = "1";
    public string Token { get; init; } = "";
    public string Email { get; init; } = "user@mail.domain";
    public string GatewayAddress { get; init; } = "";
    public string GatewayPort { get; init; } = "";
    public string PcRoom { get; init; } = "-1";
    public string HomeUrl { get; init; } = "o2jam.nopp.co.kr";
    public string Rank { get; init; } = "0";
    public string NoticeUrl { get; init; } = "http://o2jam.nopp.co.kr/client/bbs_patch_notice_nopp.html";
    public string Membership { get; init; } = "O2JAM";

    public static AuthParameters Parse(string plaintext)
    {
        string[] fields = ParseFields(plaintext);
        return new AuthParameters
        {
            FtpAddresses   = fields[0],
            FtpPort        = fields[1],
            FtpPath1       = fields[2],
            FtpPath2       = fields[3],
            GameVersion    = fields[4],
            UserIndexId    = fields[5],
            Username       = fields[6],
            Password       = fields[7],
            Level          = fields[8],
            Gender         = fields[9],
            Token          = fields[10],
            Email          = fields[11],
            GatewayAddress = fields[12],
            GatewayPort    = fields[13],
            PcRoom         = fields[14],
            HomeUrl        = fields[15],
            Rank           = fields[16],
            NoticeUrl      = fields[17],
            Membership     = fields[18],
        };
    }

    public static AuthParameters? Decrypt(string ciphertext)
    {
        string? decrypted = AuthParameterRsaCipher.Decrypt(ciphertext);
        return decrypted != null ? Parse(decrypted) : null;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        WriteField(sb, FtpAddresses);
        WriteField(sb, FtpPort);
        WriteField(sb, FtpPath1);
        WriteField(sb, FtpPath2);
        WriteField(sb, GameVersion);
        WriteField(sb, UserIndexId);
        WriteField(sb, Username);
        WriteField(sb, Password);
        WriteField(sb, Level);
        WriteField(sb, Gender);
        WriteField(sb, Token);
        WriteField(sb, Email);
        WriteField(sb, GatewayAddress);
        WriteField(sb, GatewayPort);
        WriteField(sb, PcRoom);
        WriteField(sb, HomeUrl);
        WriteField(sb, Rank);
        WriteField(sb, NoticeUrl);
        WriteField(sb, Membership);
        return sb.ToString();
    }

    public string Encode() => AuthParameterRsaCipher.Encrypt(ToString());

    private static void WriteField(StringBuilder sb, string value)
    {
        sb.Append($"{value.Length:00}");
        sb.Append(value);
    }

    private static string[] ParseFields(string plaintext)
    {
        var result = new string[FieldCount];
        int pos = 0;
        for (int i = 0; i < FieldCount; i++)
        {
            if (pos + 2 > plaintext.Length)
                throw new FormatException($"Unexpected end of data at field {i}: no length prefix at position {pos}");

            int length = int.Parse(plaintext.Substring(pos, 2));
            pos += 2;

            if (pos + length > plaintext.Length)
                throw new FormatException($"Unexpected end of data at field {i}: need {length} chars at position {pos}, only {plaintext.Length - pos} available");

            result[i] = plaintext.Substring(pos, length);
            pos += length;
        }

        return result;
    }
}
