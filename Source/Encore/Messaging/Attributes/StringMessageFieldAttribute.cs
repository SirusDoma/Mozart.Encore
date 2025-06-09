namespace Encore.Messaging;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class StringMessageFieldAttribute : MessageFieldAttribute
{
    public string? Encoding { get; }

    public string? Format { get; }

    public int MaxLength { get; }

    public bool NullTerminated { get; }

    public TypeCode PrefixSizeType { get; }

    public StringMessageFieldAttribute(int order = 0, string? encoding = null, string? format = null,
        int maxLength = ushort.MaxValue, bool nullTerminated = true, TypeCode prefixSizeType = TypeCode.Empty) :
        base(order, typeof(StringMessageFieldCodec))
    {
        Order          = order;
        Encoding       = encoding;
        Format         = format;
        MaxLength      = maxLength;
        NullTerminated = nullTerminated;
        PrefixSizeType = prefixSizeType;

        if (prefixSizeType != TypeCode.Empty && IsInteger(prefixSizeType))
            throw new ArgumentOutOfRangeException(nameof(prefixSizeType), "Prefix data type must be an integer");
    }

    private static bool IsInteger(TypeCode code)
    {
        switch (code)
        {
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                return true;
            default:
                return false;
        }
    }
}
