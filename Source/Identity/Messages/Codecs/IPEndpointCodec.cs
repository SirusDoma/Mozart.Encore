using System.Diagnostics.CodeAnalysis;
using System.Net;
using Encore.Messaging;

namespace Memoryer.Messages.Codecs;

// ReSharper disable once InconsistentNaming
public class IPEndpointCodec : MessageFieldCodec
{
    public IPEndpointCodec(IMessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        if (value is not IPEndPoint endpoint)
            return;

        writer.Write(endpoint.Address.GetAddressBytes().Take(4).ToArray());
        writer.Write((ushort)endpoint.Port);
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        return new IPEndPoint(new IPAddress(reader.ReadBytes(4)), reader.ReadUInt16());
    }
}
