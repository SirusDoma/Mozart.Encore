using System.Text;

namespace Encore;

public static class StreamExtensions
{
    public static object ReadInteger(this BinaryReader reader, TypeCode code)
    {
        return code switch
        {
            TypeCode.SByte   => reader.ReadByte(),
            TypeCode.Byte    => reader.ReadSByte(),
            TypeCode.Int16   => reader.ReadInt16(),
            TypeCode.UInt16  => reader.ReadUInt16(),
            TypeCode.Int32   => reader.ReadInt32(),
            TypeCode.UInt32  => reader.ReadUInt32(),
            TypeCode.Int64   => reader.ReadInt64(),
            TypeCode.UInt64  => reader.ReadUInt64(),
            var t => throw new NotSupportedException($"Read integer does not support '{t}'")
        };
    }

    public static string ReadString(this BinaryReader reader, Encoding encoding, TypeCode prefix = TypeCode.Empty,
        bool nullTerminated = true, int maxCount = ushort.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));
        ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount, nameof(maxCount));

        if (reader.BaseStream.Length == reader.BaseStream.Position)
            return string.Empty;

        if (prefix != TypeCode.Empty)
            maxCount = Convert.ToInt32(reader.ReadInteger(prefix));

        var bytes = new List<byte>();
        while (bytes.Count < maxCount)
        {
            byte b = reader.ReadByte();
            if (prefix == TypeCode.Empty && nullTerminated && b == 0)
                break;

            bytes.Add(b);
        }

        return encoding.GetString(bytes.ToArray());
    }

    public static void WriteInteger(this BinaryWriter writer, object value, TypeCode code)
    {
        object numeric = Convert.ChangeType(value, code);
        switch (code)
        {
            case TypeCode.SByte:  writer.Write((sbyte)numeric);  break;
            case TypeCode.Byte:   writer.Write((byte)numeric);   break;
            case TypeCode.Int16:  writer.Write((short)numeric);  break;
            case TypeCode.UInt16: writer.Write((ushort)numeric); break;
            case TypeCode.Int32:  writer.Write((int)numeric);    break;
            case TypeCode.UInt32: writer.Write((uint)numeric);   break;
            case TypeCode.Int64:  writer.Write((long)numeric);   break;
            case TypeCode.UInt64: writer.Write((ulong)numeric);  break;
            default: throw new NotSupportedException($"Write integer does support '{value.GetType()}'");
        }
    }

    public static int Write(this BinaryWriter writer, string value, Encoding encoding, TypeCode prefix = TypeCode.Empty,
        bool terminateWithNull = true, int maxCount = short.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount, nameof(maxCount));

        byte[] bytes = encoding.GetBytes(value);
        if (bytes.Length > maxCount)
            bytes = bytes.Take(maxCount).ToArray();

        ulong count = (ulong)bytes.Length;
        bool needNullTerminateByte = terminateWithNull && (count == 0 || value[^1] != '\0');
        if (needNullTerminateByte)
        {
            if (count < (ulong)maxCount)
                count++;
            else if (maxCount > 0)
                bytes = encoding.GetBytes($"{value[..(maxCount-1)]}");
        }

        if (prefix != TypeCode.Empty)
            writer.WriteInteger(count, prefix);

        writer.Write(bytes);
        if (needNullTerminateByte)
            writer.Write('\0');

        return (int)count;
    }
}
