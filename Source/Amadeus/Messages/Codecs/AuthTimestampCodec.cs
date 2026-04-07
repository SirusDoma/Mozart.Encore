using System.Diagnostics.CodeAnalysis;
using System.Text;
using Encore.Messaging;

namespace Amadeus.Messages.Codecs;

public class AuthTimestampCodec : MessageFieldCodec
{
    public AuthTimestampCodec(MessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        var expiry = (DateTime)value;
        writer.Write(Encoding.UTF8.GetBytes(expiry.ToString("yyyyMMddHHmm")));
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        throw new NotSupportedException();
    }
}
