using System.Diagnostics.CodeAnalysis;
using System.Text;
using Amadeus.Messages.Requests;
using Encore;
using Encore.Messaging;

namespace Amadeus.Messages.Codecs;

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

        long remaining = stream.Length - stream.Position;
        return (strings.Count, remaining) switch
        {
            (2, 0) => new AuthRequest.EGamesCredential
            {
                Token    = strings[0],
                ClientId = strings[1]
            },
            (3, 1) => new AuthRequest.GamaniaCredential
            {
                UserId   = strings[0],
                Token    = strings[1] + strings[2],
                ClientId = string.Empty,
                Unknown  = reader.ReadByte() != 0
            },
            _ => throw new FormatException()
        };
    }
}
