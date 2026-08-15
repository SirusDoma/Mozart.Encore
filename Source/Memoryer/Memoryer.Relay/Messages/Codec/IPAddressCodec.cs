using System.Diagnostics.CodeAnalysis;
using System.Net;
using Encore.Messaging;

namespace Memoryer.Relay.Messages.Codecs;

// ReSharper disable once InconsistentNaming
public class IPAddressCodec : MessageFieldCodec
{
    public IPAddressCodec(IMessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        if (value is not IPAddress addr)
            return;

        writer.Write(addr.GetAddressBytes().Take(4).ToArray());
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        return new IPAddress(reader.ReadBytes(4));
    }
}
