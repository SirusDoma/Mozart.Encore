namespace Encore;

public class MemoryExtendedStream : MemoryStream
{
    private readonly BinaryReader _reader;
    private readonly BinaryWriter _writer;

    public MemoryExtendedStream()
        : base()
    {
        _reader = new BinaryReader(this);
        _writer = new BinaryWriter(this);
    }

    public MemoryExtendedStream(byte[] buffer)
        : base(buffer)
    {
        _reader = new BinaryReader(this);
        _writer = new BinaryWriter(this);
    }

    public MemoryExtendedStream(byte[] buffer, bool writable)
        : base(buffer, writable)
    {
        _reader = new BinaryReader(this);
        _writer = new BinaryWriter(this);
    }

    public MemoryExtendedStream(byte[] buffer, int index, int count)
        : base(buffer, index, count)
    {
        _reader = new BinaryReader(this);
        _writer = new BinaryWriter(this);
    }

    public MemoryExtendedStream(byte[] buffer, int index, int count, bool writable)
        : base(buffer, index, count, writable)
    {
        _reader = new BinaryReader(this);
        _writer = new BinaryWriter(this);
    }

    public MemoryExtendedStream(byte[] buffer, int index, int count, bool writable, bool publiclyVisible)
        : base(buffer, index, count, writable, publiclyVisible)
    {
        _reader = new BinaryReader(this);
        _writer = new BinaryWriter(this);
    }

    public MemoryExtendedStream(int capacity)
        : base(capacity)
    {
        _reader = new BinaryReader(this);
        _writer = new BinaryWriter(this);
    }

    public Half ReadHalf()
    {
        return _reader.ReadHalf();
    }

    public char ReadChar()
    {
        return _reader.ReadChar();
    }

    public sbyte ReadSByte()
    {
        return _reader.ReadSByte();
    }

    public short ReadInt16()
    {
        return _reader.ReadInt16();
    }

    public ushort ReadUint16()
    {
        return _reader.ReadUInt16();
    }

    public int ReadInt32()
    {
        return _reader.ReadInt32();
    }

    public uint ReadUInt32()
    {
        return _reader.ReadUInt32();
    }

    public long ReadInt64()
    {
        return _reader.ReadInt64();
    }

    public ulong ReadUInt64()
    {
        return _reader.ReadUInt64();
    }

    public float ReadSingle()
    {
        return _reader.ReadSingle();
    }

    public double ReadDouble()
    {
        return _reader.ReadDouble();
    }

    public decimal ReadDecimal()
    {
        return _reader.ReadDecimal();
    }

    public string ReadString()
    {
        return _reader.ReadString();
    }

    public char[] Read(int count)
    {
        return _reader.ReadChars(count);
    }

    public int Read(Span<char> buffer)
    {
        return _reader.Read(buffer);
    }

    public int Read(char[] chars, int index, int count)
    {
        return _reader.Read(chars, index, count);
    }

    public void Write(Half value)
    {
        _writer.Write(value);
    }

    public void Write(char value)
    {
        _writer.Write(value);
    }

    public void Write(bool value)
    {
        _writer.Write(value);
    }

    public void Write(byte value)
    {
        _writer.Write(value);
    }

    public void Write(sbyte value)
    {
        _writer.Write(value);
    }

    public void Write(short value)
    {
        _writer.Write(value);
    }

    public void Write(ushort value)
    {
        _writer.Write(value);
    }

    public void Write(int value)
    {
        _writer.Write(value);
    }

    public void Write(uint value)
    {
        _writer.Write(value);
    }

    public void Write(long value)
    {
        _writer.Write(value);
    }

    public void Write(ulong value)
    {
        _writer.Write(value);
    }

    public void Write(float value)
    {
        _writer.Write(value);
    }

    public void Write(double value)
    {
        _writer.Write(value);
    }

    public void Write(string value)
    {
        _writer.Write(value);
    }

    public void Write(decimal value)
    {
        _writer.Write(value);
    }

    public void Write(byte[] buffer)
    {
        _writer.Write(buffer);
    }

    public void Write(char[] chars)
    {
        _writer.Write(chars);
    }

    public void Write(ReadOnlySpan<char> chars)
    {
        _writer.Write(chars);
    }

    public void Write(char[] chars, int index, int count)
    {
        _writer.Write(chars, index, count);
    }

    public override void Flush()
    {
        base.Flush();
        _writer.Flush();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _reader.Dispose();
        _writer.Dispose();
    }
}
