using System.Diagnostics.CodeAnalysis;
using System.Text;
using Encore;
using Encore.Messaging;
using Mozart.Metadata;

namespace Memoryer.Messages.Codecs;

public class AuthGenderCodec : MessageFieldCodec
{
    public AuthGenderCodec(MessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        throw new NotSupportedException();
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        string code = reader.ReadString(Encoding.UTF8).Trim('\0');

        const StringComparison comp = StringComparison.InvariantCultureIgnoreCase;
        if (code.Equals("m", comp) || code.Equals("1", comp))
            return Gender.Male;

        if (code.Equals("f", comp) || code.Equals("0", comp))
            return Gender.Female;

        return Gender.Any;
    }
}
