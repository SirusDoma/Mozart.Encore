using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Encore.Messaging;

public interface IMessageFramerFactory
{
    IMessageFramer CreateFramer(NetworkStream stream);
}

public class SizePrefixedMessageFramerFactory<TSize> : IMessageFramerFactory
    where TSize : unmanaged, IBinaryInteger<TSize>, IConvertible
{
    public IMessageFramer CreateFramer(NetworkStream stream)
    {
        return new SizePrefixedMessageFramer<TSize>(stream);
    }
}

public interface IMessageFramer
{
    Task<int> ReadFrame(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    Task<int> ReadFrame(Memory<byte> buffer, CancellationToken cancellationToken);

    ValueTask WriteFrame(byte[] payload, CancellationToken cancellationToken);

    ValueTask WriteFrame(Memory<byte> payload, CancellationToken cancellationToken);
}

public class SizePrefixedMessageFramer<TSize> : IMessageFramer
    where TSize : unmanaged, IBinaryInteger<TSize>, IConvertible
{
    private readonly NetworkStream _stream;

    public SizePrefixedMessageFramer(NetworkStream stream)
    {
        _stream = stream;
    }

    public Task<int> ReadFrame(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));
        ArgumentOutOfRangeException.ThrowIfLessThan((uint)count, (uint)(buffer.Length - offset), nameof(count));

        return ReadFrame(buffer.AsMemory(offset, count), cancellationToken);
    }

    public async Task<int> ReadFrame(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        if (!_stream.CanRead)
            return 0;

        byte[] prefix = new byte[default(TSize).GetByteCount()];
        await _stream.ReadExactlyAsync(prefix, cancellationToken).ConfigureAwait(false);

        var tsize = MemoryMarshal.Read<TSize>(prefix);
        if (TSize.IsZero(tsize))
            return 0; // Something wrong..

        int size = tsize.ToInt32(null) - default(TSize).GetByteCount();
        if (size == 0)
            return 0;

        if (size < 0)
            throw new FormatException("Cannot read frame with negative size");

        if (size > buffer.Length)
            throw new OutOfMemoryException("The specified buffer length is less than the frame size");

        await _stream.ReadExactlyAsync(buffer[..size], cancellationToken).ConfigureAwait(false);
        return size;
    }

    public ValueTask WriteFrame(byte[] payload, CancellationToken cancellationToken)
    {
        return WriteFrame(payload.AsMemory(), cancellationToken);
    }

    public async ValueTask WriteFrame(Memory<byte> payload, CancellationToken cancellationToken)
    {
        if (!_stream.CanWrite)
            return;

        byte[] prefix = new byte[default(TSize).GetByteCount()];

        try
        {
            if (TSize.CreateSaturating(payload.Length + prefix.Length).WriteLittleEndian(prefix) != prefix.Length)
                throw new NotSupportedException("Prefix bandwith mismatch");
        }
        catch (NotSupportedException ex)
        {
            throw new NotSupportedException(
                $"Failed to encode size prefix ({payload.Length}) to {nameof(TSize)}", ex
            );
        }

        await _stream.WriteAsync(prefix.Concat(payload.ToArray()).ToArray(), cancellationToken).ConfigureAwait(false);
    }
}