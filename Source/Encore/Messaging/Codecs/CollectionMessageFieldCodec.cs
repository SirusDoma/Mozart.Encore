using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Encore.Messaging;

public sealed class CollectionMessageFieldCodec : MessageFieldCodec
{
    private readonly IMessageFieldCodec _defaultCodec;

    public CollectionMessageFieldCodec(IMessageFieldCodec defaultCodec, IMessageFieldAttribute attribute)
        : base(SingleOrDefaultAttribute(attribute))
    {
        _defaultCodec = defaultCodec;
    }

    private static IMessageFieldAttribute SingleOrDefaultAttribute(IMessageFieldAttribute attribute)
    {
        if (attribute is not CollectionMessageFieldAttribute)
        {
            if (attribute.GetType() == typeof(MessageFieldAttribute) || (
                    attribute.GetType().IsGenericType &&
                    attribute.GetType().GetGenericTypeDefinition() == typeof(MessageFieldAttribute<>)))
            {
                return new CollectionMessageFieldAttribute(attribute.Order);
            }

            throw new ArgumentOutOfRangeException(nameof(attribute), attribute,
                "must be an instance of 'CollectionMessageFieldAttribute'");
        }

        return attribute;
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        var attribute = (CollectionMessageFieldAttribute)Attribute;

        var prefixSizeType = attribute.PrefixSizeType;
        int minCount       = attribute.MinCount;
        int maxCount       = attribute.MaxCount;
        var codec          = _defaultCodec;

        var elementType = GetEnumerableElementType(sourceType);
        var items  = Resize(
            ((IEnumerable)value).Cast<object>().ToList(), minCount, maxCount, CreateDefaultValue(elementType)
        );

        if (prefixSizeType != TypeCode.Empty)
            writer.WriteInteger(items.Count, prefixSizeType);

        foreach (object item in items)
        {
            codec.Encode(writer, item, elementType);
        }
    }

    [UnconditionalSuppressMessage("AOT", "IL3050",
        Justification = "Only support primitive and registered IMessage types")]
    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        var attribute = (CollectionMessageFieldAttribute)Attribute;

        var prefixSizeType = attribute.PrefixSizeType;
        int minCount       = attribute.MinCount;
        int maxCount       = attribute.MaxCount;
        var codec          = _defaultCodec;

        int count = minCount;
        int realCount = minCount;
        if (prefixSizeType != TypeCode.Empty)
        {
            realCount = Convert.ToInt32(reader.ReadInteger(prefixSizeType));
            count = Math.Min(Math.Max(realCount, minCount), maxCount);
        }

        var elementType = GetEnumerableElementType(targetType);

        object[] items = new object[count];
        for (int i = 0; i < realCount; i++)
            items[i] = codec.Decode(reader, elementType);

        for (int i = 0; i < Math.Abs(count - realCount); i++)
            items[i] = CreateDefaultValue(elementType)!;

        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, count);
            for (int i = 0; i < count; i++)
                array.SetValue(items[i], i);

            return array;
        }

        List<Type> genericEnumerableTypes = targetType.IsGenericType  ? [targetType] : [];
        genericEnumerableTypes.AddRange(targetType.GetInterfaces());

        if (genericEnumerableTypes.Any(e =>
                e.IsInterface && e.GetGenericTypeDefinition().IsAssignableTo(typeof(IEnumerable<>))))
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType)!;

            foreach (object item in items)
                list.Add(item);

            return list;
        }

        throw new NotSupportedException($"Collection type {targetType.Name} is not supported");
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private Type GetEnumerableElementType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        var ex = new ArgumentException($"Cannot determine element type for {type.Name}");

        if (type.IsArray)
            return ToSupportedFieldType(type.GetElementType()) ?? throw ex;

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableInterface != null)
            return ToSupportedFieldType(enumerableInterface.GetGenericArguments()[0]) ?? throw ex;

        if (type.IsGenericType &&
            typeof(IEnumerable).IsAssignableFrom(type.GetGenericTypeDefinition()))
        {
            return ToSupportedFieldType(type.GetGenericArguments()[0]) ?? throw ex;
        }

        throw ex;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private Type? ToSupportedFieldType(Type? type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Object   => _defaultCodec is IMessageCodec d ? d.GetRegisteredType(type) : null,
            TypeCode.Boolean  => typeof(bool),
            TypeCode.Char     => typeof(char),
            TypeCode.SByte    => typeof(sbyte),
            TypeCode.Byte     => typeof(byte),
            TypeCode.Int16    => typeof(short),
            TypeCode.UInt16   => typeof(ushort),
            TypeCode.Int32    => typeof(int),
            TypeCode.UInt32   => typeof(uint),
            TypeCode.Int64    => typeof(long),
            TypeCode.UInt64   => typeof(ulong),
            TypeCode.Single   => typeof(float),
            TypeCode.Double   => typeof(double),
            TypeCode.Decimal  => typeof(decimal),
            TypeCode.String   => typeof(string),
            _ => null
        };
    }

    private static object? CreateDefaultValue(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
        Type type
    )
    {
        ArgumentNullException.ThrowIfNull(type, nameof(type));

        if (type == typeof(bool))    return false;
        if (type == typeof(char))    return '\0';
        if (type == typeof(byte))    return 0;
        if (type == typeof(sbyte))   return 0;
        if (type == typeof(short))   return 0;
        if (type == typeof(ushort))  return 0;
        if (type == typeof(int))     return 0;
        if (type == typeof(uint))    return 0;
        if (type == typeof(long))    return 0;
        if (type == typeof(ulong))   return 0;
        if (type == typeof(float))   return 0;
        if (type == typeof(double))  return 0;
        if (type == typeof(decimal)) return 0;
        if (type == typeof(string))  return string.Empty;

        if (type.IsAssignableTo(typeof(IMessage)) && type.GetConstructor(Type.EmptyTypes) != null)
            return Activator.CreateInstance(type);

        return null;
    }

    private static IList Resize(IList list, int minCount, int maxCount, object? padValue)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentOutOfRangeException.ThrowIfNegative(minCount, nameof(minCount));
        ArgumentOutOfRangeException.ThrowIfNegative(maxCount, nameof(minCount));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minCount, maxCount, nameof(maxCount));

        while (list.Count > maxCount)
        {
            list.RemoveAt(list.Count - 1);
        }

        while (list.Count < minCount)
        {
            list.Add(padValue);
        }

        return list;
    }
}