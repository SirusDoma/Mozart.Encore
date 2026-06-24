using Encore.Messaging;

namespace Memoryer.Relay.Messaging;

public sealed record UdpRelayFrame(
    UdpRelayPacketType PacketType,
    byte               Sequence,
    byte[]             Payload
);

public interface IUdpRelayFrameCodec
{
    byte[] EncodeData<TMessage>(UdpRelayPacketType packetType, byte sequence, TMessage message)
        where TMessage : class, IMessage;

    byte[] EncodeData(UdpRelayPacketType packetType, byte sequence, IMessage message);

    byte[] EncodeAck(byte sequence);

    UdpRelayFrame Decode(byte[] datagram);
}
