using System.Buffers.Binary;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using Encore.Messaging;
using Encore.Server;
using Encore.Sessions;
using Microsoft.Extensions.Options;

namespace Memoryer.Relay.Sessions;

public class UdpRelaySession : UdpSession, IRelayPeer
{
    private readonly UdpRelayPeer _peer;
    private readonly IMessageCodec _codec;

    public UdpRelaySession(
        UdpClient transport,
        IOptions<RelayOptions> options,
        ICommandDispatcher dispatcher,
        UdpReceiveResult result,
        IUdpRelayPeerRegistry registry,
        IMessageCodec codec
    )
        : base(transport, new UdpOptions
        {
            Address           = options.Value.Endpoints.FirstOrDefault()?.Address ?? "0.0.0.0",
            Port              = 0,
            ReceiveBufferSize = options.Value.PacketBufferSize
        }, dispatcher, result)
    {
        _codec = codec;
        _peer  = registry.GetOrCreate(transport, result.RemoteEndPoint);
    }

    public IPEndPoint LocalEndPoint => _peer.LocalEndPoint;

    public bool Authorized => _peer.Authorized;

    public void Authorize<T>(T token) => _peer.Authorize(token);

    public T GetAuthorizedToken<T>() => _peer.GetAuthorizedToken<T>();

    private byte PacketType { get; set; }

    protected override async Task OnFrameReceived(byte[] datagram, CancellationToken cancellationToken)
    {
        // TODO: Proper re-transmission + deduplicate

        if (datagram is [0x03, _, _, _])
            return; // ACK from client

        using var stream = new  MemoryStream(Decompress(datagram));
        using var reader = new  BinaryReader(stream);

        PacketType = reader.ReadByte();
        byte seq   = reader.ReadByte();
        ushort prefix = reader.ReadUInt16();
        ushort code = (ushort)reader.ReadUInt32();

        byte[] payload = BitConverter.GetBytes(code).Concat(reader.ReadBytes(prefix - 4)).ToArray();

        if (seq == _peer.RecvSequence)
            return;

        // 0x03 = Ack
        _peer.RecvSequence = seq;
        await WriteFrame([0x03, _peer.RecvSequence, 0x00, 0x00], cancellationToken).ConfigureAwait(false);

        await base.OnFrameReceived(payload, cancellationToken);
    }

    public async Task WriteMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        _codec.Register<TMessage>();
        byte[] payload = _codec.Encode(message);
        ushort command = BinaryPrimitives.ReadUInt16LittleEndian(payload);
        payload = payload[2..];

        using var stream = new MemoryStream();
        await using var writer = new  BinaryWriter(stream);

        writer.Write(PacketType);
        writer.Write(++_peer.SendSequence);
        writer.Write((ushort)(payload.Length + 4));
        writer.Write((uint)command);
        writer.Write(payload);

        writer.Flush();
        await stream.FlushAsync(cancellationToken);

        await WriteFrame(Compress(stream.ToArray()), cancellationToken).ConfigureAwait(false);
    }

    private byte[] Decompress(byte[] datagram)
    {
        ushort prefix = BinaryPrimitives.ReadUInt16LittleEndian(datagram);

        byte[] compressed = datagram[2..];
        byte[] payload    = new byte[prefix];

        using var input = new MemoryStream(compressed, writable: false);
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

    private byte[] Compress(byte[] payload)
    {
        using var stream = new MemoryStream();
        using (var zlib = new ZLibStream(stream, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(payload);

        byte[] compressed = stream.ToArray();
        compressed = BitConverter.GetBytes((ushort)payload.Length).Concat(compressed).ToArray();
        return compressed;
    }
}
