using System.Diagnostics.CodeAnalysis;
using System.Text;
using Encore;
using Encore.Messaging;
using Mozart.Messages.Requests;

namespace Mozart.Messages.Codecs;

public class AuthCredentialCodec(IMessageFieldAttribute attribute) : MessageFieldCodec(attribute)
{
    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        throw new NotSupportedException();
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        var stream  = reader.BaseStream;
        var strings = new List<string>();
        while (stream.Position < stream.Length && strings.Count < 3)
            strings.Add(reader.ReadString(Encoding.UTF8));

        return strings.Count switch
        {
            2 => new AuthRequest.EGamesCredential
            {
                Token = strings[0]
            },
            3 => new AuthRequest.GamaniaCredential
            {
                Token    = strings[1] + strings[0],
                UserId   = strings[1],
                Unknown  = strings[2]
            },
            _ => throw new FormatException()
        };
    }
}
