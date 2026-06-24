using System.Buffers.Binary;
using System.IO.Compression;
using Encore.Messaging;

namespace Memoryer.Relay.Messaging;

public class UdpRelayFrameCodec : IUdpRelayFrameCodec
{
    private const int AckFrameLength = 4;

    private readonly IMessageCodec _codec;

    public UdpRelayFrameCodec(IMessageCodec codec)
    {
        _codec = codec;
    }

    public byte[] EncodeData<TMessage>(UdpRelayPacketType packetType, byte sequence, TMessage message)
        where TMessage : class, IMessage
    {
        _codec.Register<TMessage>();
        byte[] commandPayload = _codec.Encode(message);
        return EncodeDataCore(packetType, sequence, commandPayload);
    }

    public byte[] EncodeData(UdpRelayPacketType packetType, byte sequence, IMessage message)
    {
        byte[] commandPayload = _codec.Encode(message);
        return EncodeDataCore(packetType, sequence, commandPayload);
    }

    public byte[] EncodeAck(byte sequence)
    {
        return [(byte)UdpRelayPacketType.Ack, sequence, 0x00, 0x00];
    }

    public UdpRelayFrame Decode(byte[] datagram)
    {
        if (datagram.Length == AckFrameLength &&
            datagram[0] == (byte)UdpRelayPacketType.Ack)
        {
            return new UdpRelayFrame(UdpRelayPacketType.Ack, datagram[1], []);
        }

        byte[] inflated = Decompress(datagram);

        using var stream = new MemoryStream(inflated, writable: false);
        using var reader = new BinaryReader(stream);

        var packetType   = (UdpRelayPacketType)reader.ReadByte();
        byte sequence    = reader.ReadByte();
        ushort innerSize = reader.ReadUInt16();
        ushort command   = (ushort)reader.ReadUInt32();

        int bodyLength = Math.Max(0, innerSize - 4);
        byte[] body    = reader.ReadBytes(bodyLength);

        byte[] innerPayload = new byte[2 + body.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(innerPayload, command);
        Buffer.BlockCopy(body, 0, innerPayload, 2, body.Length);

        return new UdpRelayFrame(packetType, sequence, innerPayload);
    }

    private byte[] EncodeDataCore(UdpRelayPacketType packetType, byte sequence, byte[] commandPayload)
    {
        // commandPayload is `[u16 command][body]`. Split it into u32 code + body for the envelope.
        ushort command = BinaryPrimitives.ReadUInt16LittleEndian(commandPayload);
        byte[] body    = commandPayload[2..];

        using var inner       = new MemoryStream();
        using var innerWriter = new BinaryWriter(inner);

        innerWriter.Write((byte)packetType);
        innerWriter.Write(sequence);
        innerWriter.Write((ushort)(body.Length + 4));
        innerWriter.Write((uint)command);
        innerWriter.Write(body);
        innerWriter.Flush();

        byte[] uncompressed = inner.ToArray();
        return Compress(uncompressed);
    }

    private static byte[] Compress(byte[] payload)
    {
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(payload);

        byte[] body  = compressed.ToArray();
        byte[] frame = new byte[2 + body.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(frame, (ushort)payload.Length);
        Buffer.BlockCopy(body, 0, frame, 2, body.Length);
        return frame;
    }

    private static byte[] Decompress(byte[] datagram)
    {
        ushort uncompressedSize = BinaryPrimitives.ReadUInt16LittleEndian(datagram);

        byte[] payload = new byte[uncompressedSize];
        using var input = new MemoryStream(datagram, 2, datagram.Length - 2, writable: false);
        using var zlib  = new ZLibStream(input, CompressionMode.Decompress);

        int read = 0;
        while (read < payload.Length)
        {
            int n = zlib.Read(payload, read, payload.Length - read);
            if (n == 0)
                break;

            read += n;
        }

        return payload;
    }
}
