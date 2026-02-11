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
    Task<byte[]> ReadFrame(int bufferSize = 1024, CancellationToken cancellationToken = default);

    ValueTask WriteFrame(byte[] payload, CancellationToken cancellationToken);

    ValueTask WriteFrame(Memory<byte> payload, CancellationToken cancellationToken);
}

public class SizePrefixedMessageFramer<TSize> : IMessageFramer
    where TSize : unmanaged, IBinaryInteger<TSize>, IConvertible
{
    private const uint MB = 1048576;

    private readonly NetworkStream _stream;

    public SizePrefixedMessageFramer(NetworkStream stream)
    {
        _stream = stream;
    }

    public async Task<byte[]> ReadFrame(int bufferSize = 1024, CancellationToken cancellationToken = default)
    {
        if (!_stream.CanRead)
            return [];

        byte[] prefix = new byte[default(TSize).GetByteCount()];
        await _stream.ReadExactlyAsync(prefix, cancellationToken).ConfigureAwait(false);

        var tsize = MemoryMarshal.Read<TSize>(prefix);
        if (TSize.IsZero(tsize))
            return []; // Something wrong..

        int size = tsize.ToInt32(null) - default(TSize).GetByteCount();
        if (size == 0)
            return [];

        if (size < 0)
            throw new FormatException("Cannot read frame with negative size");

        if (size > 1 * MB) // 1MB
            throw new OutOfMemoryException("The the frame size is too big");

        byte[] buffer = new byte[size];
        for (int read = 0; read < size; read += bufferSize)
        {
            if (read + bufferSize > size)
                bufferSize = size - read;

            await _stream.ReadExactlyAsync(buffer, read, bufferSize, cancellationToken).ConfigureAwait(false);
        }

        return buffer;
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
