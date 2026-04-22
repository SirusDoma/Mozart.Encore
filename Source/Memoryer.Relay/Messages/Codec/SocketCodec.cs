using System.Diagnostics.CodeAnalysis;
using System.Net;
using Encore.Messaging;

namespace Memoryer.Relay.Messages.Codecs;

// ReSharper disable once InconsistentNaming
public class SocketCodec : MessageFieldCodec
{
    public SocketCodec(IMessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        if (value is not IPEndPoint endpoint)
            return;

        writer.Write((ushort)endpoint.Port);
        writer.Write((ushort)0);
        writer.Write(endpoint.Address.GetAddressBytes().Take(4).ToArray());
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        ushort port = reader.ReadUInt16();
        Console.WriteLine(reader.ReadUInt16());
        var ipv4 = new IPAddress(reader.ReadBytes(4));

        return new IPEndPoint(ipv4, port);
    }
}
