using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Encore.Messaging;

public interface IMessageFieldCodec
{
    void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType);

    object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType);
}

public abstract class MessageFieldCodec : IMessageFieldCodec
{
    protected MessageFieldCodec(IMessageFieldAttribute attribute)
    {
        Attribute = attribute;
    }

    public IMessageFieldAttribute Attribute { get; }

    public abstract void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType);

    public abstract object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType);
}

public class MessageFieldCodec<T> : MessageFieldCodec
    where T : unmanaged
{
    public MessageFieldCodec(IMessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        value = typeof(T).IsEnum ? Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T))) : value;

        var typedValue = value is T t ? t : (T)Convert.ChangeType(value, typeof(T));
        var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref typedValue, 1));
        writer.Write(bytes);
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        var bytes = reader.ReadBytes(Unsafe.SizeOf<T>());
        return MemoryMarshal.Read<T>(bytes);
    }
}
