using System.Diagnostics.CodeAnalysis;
using System.Text;
using Encore;
using Encore.Messaging;

public class StringMessageFieldCodec : MessageFieldCodec
{
    public StringMessageFieldCodec(IMessageFieldAttribute attribute)
        : base(attribute)
    {
        if (Attribute is not MessageFieldAttribute and not StringMessageFieldAttribute)
        {
            throw new ArgumentOutOfRangeException(nameof(attribute), attribute,
                "must be an instance of 'StringMessageFieldAttribute'");
        }
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        var attribute = Attribute as StringMessageFieldAttribute;

        var comparison = StringComparison.InvariantCultureIgnoreCase;
        var encoding = attribute?.Encoding switch
        {
            null => Encoding.UTF8,
            not null when attribute.Encoding.Equals("UTF-8",     comparison) ||
                          attribute.Encoding.Equals("UTF8",      comparison) => Encoding.UTF8,
            not null when attribute.Encoding.Equals("Unicode",   comparison) ||
                          attribute.Encoding.Equals("UTF-16",    comparison) ||
                          attribute.Encoding.Equals("UTF-16LE",  comparison) ||
                          attribute.Encoding.Equals("UTF16",     comparison) ||
                          attribute.Encoding.Equals("UTF16LE",   comparison) => Encoding.Unicode,
            not null when attribute.Encoding.Equals("UnicodeBE", comparison) ||
                          attribute.Encoding.Equals("UTF-16BE",  comparison) ||
                          attribute.Encoding.Equals("UTF16BE",   comparison) => Encoding.BigEndianUnicode,
            not null when attribute.Encoding.Equals("UTF-32",    comparison) ||
                          attribute.Encoding.Equals("UTF32",     comparison) => Encoding.UTF32,
            not null when attribute.Encoding.Equals("ASCII",     comparison) => Encoding.ASCII,
            _  => Encoding.GetEncoding(attribute.Encoding)
        };

        string? format      = attribute?.Format;
        int maxLength       = attribute?.MaxLength ?? ushort.MaxValue;
        bool nullTerminated = attribute?.NullTerminated ?? false;
        var prefixSizeType  = attribute?.PrefixSizeType ?? TypeCode.Empty;

        var fieldType = value.GetType();
        if (fieldType.IsEnum)
        {
            string name = Enum.GetName(value.GetType(), value)!;
            writer.Write(name, encoding, prefixSizeType, nullTerminated, maxLength);
        }
        else if (fieldType.IsAssignableTo(typeof(DateTime)))
        {
            string formatted = ((DateTime)value).ToString(format);
            writer.Write(formatted, encoding, prefixSizeType, nullTerminated, maxLength);
        }
        else if (fieldType.IsAssignableTo(typeof(TimeSpan)))
        {
            string formatted = ((TimeSpan)value).ToString(format);
            writer.Write(formatted, encoding, prefixSizeType, nullTerminated, maxLength);
        }
        else if (fieldType == typeof(string))
        {
            writer.Write((string)value, encoding, prefixSizeType, nullTerminated, maxLength);
        }
        else
            throw new NotSupportedException($"{GetType().Name} does not support encoding '{fieldType.Name}'");
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        var attribute = Attribute as StringMessageFieldAttribute;

        var comparison = StringComparison.InvariantCultureIgnoreCase;
        var encoding = attribute?.Encoding switch
        {
            null => Encoding.UTF8,
            not null when attribute.Encoding.Equals("UTF-8",    comparison) ||
                          attribute.Encoding.Equals("UTF8",     comparison) => Encoding.UTF8,
            not null when attribute.Encoding.Equals("UTF-16",   comparison) ||
                          attribute.Encoding.Equals("UTF16",    comparison) => Encoding.Unicode,
            not null when attribute.Encoding.Equals("UTF-16BE", comparison) ||
                          attribute.Encoding.Equals("UTF16BE",  comparison) => Encoding.BigEndianUnicode,
            not null when attribute.Encoding.Equals("UTF-32",   comparison) ||
                          attribute.Encoding.Equals("UTF32",    comparison) => Encoding.UTF32,
            not null when attribute.Encoding.Equals("ASCII",    comparison) => Encoding.ASCII,
            _  => Encoding.GetEncoding(attribute.Encoding)
        };

        string? format      = attribute?.Format;
        int maxLength       = attribute?.MaxLength ?? ushort.MaxValue;
        bool nullTerminated = attribute?.NullTerminated ?? false;
        var prefixSizeType  = attribute?.PrefixSizeType ?? TypeCode.Empty;

        string value = reader.ReadString(encoding, prefixSizeType, nullTerminated, maxLength).Trim('\0');
        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value);
        }
        if (targetType.IsAssignableTo(typeof(DateTime)))
        {
            if (string.IsNullOrEmpty(format))
                return DateTime.Parse(value);

            return DateTime.ParseExact(value, format!, null);
        }
        if (targetType.IsAssignableTo(typeof(TimeSpan)))
        {
            if (string.IsNullOrEmpty(format))
                return TimeSpan.Parse(value);

            return TimeSpan.ParseExact(value, format!, null);
        }
        if (targetType == typeof(string))
        {
            return value;
        }

        throw new NotSupportedException($"{GetType().Name} does not support encoding '{targetType}'");
    }
}
