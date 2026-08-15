namespace Memoryer.Relay.Messaging;

public enum UdpRelayPacketType : byte
{
    Unreliable = 0x01,
    Reliable   = 0x02,
    Ack        = 0x03,
}
