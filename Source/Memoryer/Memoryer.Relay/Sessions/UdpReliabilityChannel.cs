using System.Collections.Concurrent;
using Encore.Messaging;
using Memoryer.Relay.Messaging;

namespace Memoryer.Relay.Sessions;

public sealed class UdpReliabilityChannel : IAsyncDisposable
{
    private readonly UdpRelayPeer            _peer;
    private readonly IUdpRelayFrameCodec     _codec;
    private readonly TimeSpan                _retransmissionInterval;
    private readonly int                     _maxRetransmissionAttempts;
    private readonly ConcurrentDictionary<byte, PendingSend> _pending = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task                    _retransmitLoop;
    private readonly object                  _sendLock = new();

    private byte? _lastReceivedSeq;
    private byte  _nextSendSeq;

    public UdpReliabilityChannel(
        UdpRelayPeer        peer,
        IUdpRelayFrameCodec codec,
        TimeSpan            retransmissionInterval,
        int                 maxRetransmissionAttempts
    )
    {
        _peer                      = peer;
        _codec                     = codec;
        _retransmissionInterval    = retransmissionInterval;
        _maxRetransmissionAttempts = maxRetransmissionAttempts;
        _retransmitLoop            = Task.Run(() => RunRetransmitLoop(_cts.Token));
    }

    public async Task SendReliableAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        byte   sequence = AllocateSendSequence();
        byte[] datagram = _codec.EncodeData(UdpRelayPacketType.Reliable, sequence, message);

        _pending[sequence] = new PendingSend(datagram, DateTime.UtcNow, Attempts: 0);

        await _peer.WriteFrame(datagram, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendUnreliableAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class, IMessage
    {
        byte   sequence = AllocateSendSequence();
        byte[] datagram = _codec.EncodeData(UdpRelayPacketType.Unreliable, sequence, message);

        await _peer.WriteFrame(datagram, cancellationToken).ConfigureAwait(false);
    }

    public async Task<byte[]?> ReceiveAsync(byte[] datagram, CancellationToken cancellationToken)
    {
        var frame = _codec.Decode(datagram);

        switch (frame.PacketType)
        {
            case UdpRelayPacketType.Ack:
                _pending.TryRemove(frame.Sequence, out _);
                return null;

            case UdpRelayPacketType.Unreliable:
                return frame.Payload;

            case UdpRelayPacketType.Reliable:
                // Always ack, including duplicates, so the sender can stop retransmitting
                // even when a previous ack was lost in flight.
                await _peer.WriteFrame(_codec.EncodeAck(frame.Sequence), cancellationToken)
                    .ConfigureAwait(false);

                if (_lastReceivedSeq == frame.Sequence)
                    return null;

                _lastReceivedSeq = frame.Sequence;
                return frame.Payload;

            default:
                return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync().ConfigureAwait(false);
        try
        {
            await _retransmitLoop.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        _cts.Dispose();
        _pending.Clear();
    }

    private byte AllocateSendSequence()
    {
        lock (_sendLock)
        {
            return ++_nextSendSeq;
        }
    }

    private async Task RunRetransmitLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_retransmissionInterval, cancellationToken).ConfigureAwait(false);

                var now      = DateTime.UtcNow;
                var deadline = now - _retransmissionInterval;

                foreach (var (sequence, pending) in _pending)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (pending.SentAt > deadline)
                        continue;

                    if (_maxRetransmissionAttempts > 0 && pending.Attempts >= _maxRetransmissionAttempts)
                    {
                        _pending.TryRemove(sequence, out _);
                        continue;
                    }

                    try
                    {
                        await _peer.WriteFrame(pending.Datagram, cancellationToken).ConfigureAwait(false);
                        _pending[sequence] = pending with { SentAt = now, Attempts = pending.Attempts + 1 };
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch
                    {
                        // Transient socket errors; try again on the next tick.
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private sealed record PendingSend(byte[] Datagram, DateTime SentAt, int Attempts);
}
