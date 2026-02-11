using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Encore.Messaging;

public interface IMessageCodec
{
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type? GetRegisteredType(Type? type);

    void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IMessage;

    void Register<T>(T command)
        where T : Enum;

    void Register<T>(T command, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
        where T : Enum;

    void Register([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type);

    byte[] Encode<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T message)
        where T : class, IMessage;

    byte[] Encode(IMessage message);

    byte[] EncodeCommand(Enum command);

    T Decode<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(byte[] data)
        where T : class, IMessage, new();

    IMessage? Decode(byte[] data);

    Enum DecodeCommand(byte[] data);
}

public partial class DefaultMessageCodec : IMessageCodec, IMessageFieldCodec
{
    private static readonly Dictionary<Type, Func<IMessageFieldAttribute, MessageFieldCodec>> StandardFieldCodecs = new()
    {
        { typeof(bool),     (attribute) => new MessageFieldCodec<bool>(attribute)     },
        { typeof(char),     (attribute) => new MessageFieldCodec<char>(attribute)     },
        { typeof(byte),     (attribute) => new MessageFieldCodec<byte>(attribute)     },
        { typeof(sbyte),    (attribute) => new MessageFieldCodec<sbyte>(attribute)    },
        { typeof(short),    (attribute) => new MessageFieldCodec<short>(attribute)    },
        { typeof(ushort),   (attribute) => new MessageFieldCodec<ushort>(attribute)   },
        { typeof(int),      (attribute) => new MessageFieldCodec<int>(attribute)      },
        { typeof(uint),     (attribute) => new MessageFieldCodec<uint>(attribute)     },
        { typeof(long),     (attribute) => new MessageFieldCodec<long>(attribute)     },
        { typeof(ulong),    (attribute) => new MessageFieldCodec<ulong>(attribute)    },
        { typeof(float),    (attribute) => new MessageFieldCodec<float>(attribute)    },
        { typeof(double),   (attribute) => new MessageFieldCodec<double>(attribute)   },
        { typeof(decimal),  (attribute) => new MessageFieldCodec<decimal>(attribute)  },
        { typeof(string),   (attribute) => new StringMessageFieldCodec(attribute)     },
        { typeof(DateTime), (attribute) => new MessageFieldCodec<DateTime>(attribute) },
        { typeof(TimeSpan), (attribute) => new MessageFieldCodec<TimeSpan>(attribute) },
        { typeof(Guid),     (attribute) => new MessageFieldCodec<Guid>(attribute)     }
    };

    private readonly Dictionary<ushort, Type> _types = new();
    private readonly Dictionary<ushort, Enum> _commands = [];

    private readonly HashSet<Type> _typeSet = [];

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? GetRegisteredType(Type? type) =>
        type != null && _typeSet.TryGetValue(type, out var v) ? v : null;

    public void Register<T>(T command) where T : Enum
    {
        ushort code = Convert.ToUInt16(command);
        if (!_commands.TryAdd(code, command) && !Equals(command, _commands[code]))
            throw new InvalidOperationException($"'{code:X4}' is already bound to '{_commands[code]}'");
    }

    public void Register<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>()
        where T : class, IMessage
    {
        var type = typeof(T);
        ushort command = Convert.ToUInt16(T.Command);

        if (!_types.TryAdd(command, type) && type != _types[command])
            throw new InvalidOperationException($"'{command:X4}' is already bound to '{_types[command].Name}'");

        RegisterType(type);
    }

    public void Register([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        if (!typeof(IMessage).IsAssignableFrom(type) || type.IsInterface)
            throw new ArgumentOutOfRangeException( nameof(type), $"Type '{type.Name}' must implement IMessage.");

        var property = type.GetProperty("Command",
            BindingFlags.Public | BindingFlags.Static);

        if (property == null)
            throw new InvalidOperationException($"Type '{type.Name}' does not have a static Command property");

        ushort command = Convert.ToUInt16((Enum)property.GetValue(null)!);
        if (!_types.TryAdd(command, type) && type != _types[command])
            throw new InvalidOperationException($"'{command:X4}' is already bound to '{_types[command].Name}'");

        RegisterType(type);
    }

    public void Register<T>(T command, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
        where T : Enum
    {
        if (!typeof(IMessage).IsAssignableFrom(type) || type.IsInterface)
            throw new ArgumentException($"Type '{type.Name}' must implement IMessage.", nameof(type));

        ushort code = Convert.ToUInt16(command);
        if (!_types.TryAdd(code, type) && type != _types[code])
            throw new InvalidOperationException($"'{code:X4}' is already bound to '{_types[code].Name}'");

        RegisterType(type);
    }


    private void RegisterType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        _typeSet.Add(type);
        foreach (var member in GetSerializableMembers(type).OrderBy(m => m.Attribute.Order))
        {
            var memberType = member.MemberType;
            if (typeof(IEnumerable).IsAssignableFrom(member.MemberType) && member.MemberType != typeof(string))
                memberType = GetEnumerableElementType(member.MemberType);

            if (memberType.IsAssignableTo(typeof(IMessage)))
                _typeSet.Add(memberType);
        }
    }

    public byte[] Encode<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T message)
        where T : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        ushort command = Convert.ToUInt16(T.Command);
        writer.Write(command);

        EncodeMessage(writer, message, typeof(T));

        return stream.ToArray();
    }

    public byte[] Encode(IMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        if (!_typeSet.TryGetValue(message.GetType(), out var type))
            throw new NotSupportedException($"'{message.GetType()}' is not recognized");

        var property = type.GetProperty("Command",
            BindingFlags.Public | BindingFlags.Static);

        if (property == null)
            throw new InvalidOperationException($"Type '{type.Name}' does not have a static Command property");

        ushort command = Convert.ToUInt16((Enum)property.GetValue(null)!);
        writer.Write(command);

        EncodeMessage(writer, message, type);
        return stream.ToArray();
    }

    public byte[] EncodeCommand(Enum command)
    {
        ushort code = (ushort)Convert.ChangeType(command, TypeCode.UInt16);
        if (_commands.TryGetValue(code, out var cmd))
            return BitConverter.GetBytes((ushort)code);

        //throw new NotSupportedException($"Command '0x{code:X4}' is not recognized");
        return BitConverter.GetBytes((ushort)code);
    }

    public T Decode<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(byte[] data)
        where T : class, IMessage, new()
    {
        ArgumentNullException.ThrowIfNull(data);

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        ushort command  = reader.ReadUInt16();
        ushort expected = Convert.ToUInt16(T.Command);

        if (command != expected)
        {
            throw new InvalidOperationException($"Command mismatch. " +
                                                $"Expected {expected:X4} but got {command:X4}");
        }

        return (T)DecodeMessage(reader, typeof(T));
    }

    public IMessage? Decode(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        ushort command = reader.ReadUInt16();

        if (!_types.TryGetValue(command, out var messageType))
        {
            if (_commands.ContainsKey(command))
                return null;

            throw new NotSupportedException($"Command '0x{command:X4}' is not recognized");
        }

        return DecodeMessage(reader, messageType);
    }

    public Enum DecodeCommand(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        ushort command = reader.ReadUInt16();

        if (_commands.TryGetValue(command, out var cmd))
            return cmd;

        if (_types.TryGetValue(command, out var type))
        {
            var property = type.GetProperty("Command",
                BindingFlags.Public | BindingFlags.Static);

            if (property == null)
                throw new InvalidOperationException($"Type '{type.Name}' does not have a static Command property");

            return (Enum)property.GetValue(null)!;
        }

        throw new NotSupportedException($"Command '0x{command:X4}' is not recognized");
    }

    private void EncodeMessage(BinaryWriter writer, IMessage message,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        var members = GetSerializableMembers(type);

        foreach (var member in members.OrderBy(m => m.Attribute.Order))
        {
            if (!member.CanRead)
                continue;

            object? value  = member.GetValue(message);
            var codec      = member.CreateFieldCodec(this);
            var memberType = member.MemberType;
            var attribute  = member.Attribute;

            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Nullable<>))
                memberType = Nullable.GetUnderlyingType(memberType)!;

            if (codec != null && value != null)
                codec.Encode(writer, value, memberType);
            else
                EncodeValue(writer, value, memberType, attribute);
        }
    }

    private IMessage DecodeMessage(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type
    )
    {
        var message = (IMessage)Activator.CreateInstance(type)!;
        var members = GetSerializableMembers(type);

        foreach (var member in members.OrderBy(m => m.Attribute.Order))
        {
            if (!member.CanWrite)
                continue;

            object value;
            var codec      = member.CreateFieldCodec(this);
            var memberType = member.MemberType;
            var attribute  = member.Attribute;

            if (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof(Nullable<>))
                memberType = Nullable.GetUnderlyingType(memberType)!;

            if (codec != null)
                value = codec.Decode(reader, memberType);
            else
                value = DecodeValue(reader, memberType, attribute);

            member.SetValue(message, value);
        }

        return message;
    }

    private void EncodeValue(BinaryWriter writer, object? value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, IMessageFieldAttribute attribute)
    {
        // ArgumentNullException.ThrowIfNull(value);

        if (value == null)
            return;

        if (StandardFieldCodecs.TryGetValue(type, out var serializer))
        {
            serializer(attribute).Encode(writer, value, type);
            return;
        }

        if (type.IsEnum)
        {
            IMessageFieldCodec enumCodec = Type.GetTypeCode(Enum.GetUnderlyingType(type)) switch
            {
                TypeCode.Char   => new MessageFieldCodec<char>(attribute),
                TypeCode.SByte  => new MessageFieldCodec<sbyte>(attribute),
                TypeCode.Byte   => new MessageFieldCodec<byte>(attribute),
                TypeCode.Int16  => new MessageFieldCodec<short>(attribute),
                TypeCode.UInt16 => new MessageFieldCodec<ushort>(attribute),
                TypeCode.Int32  => new MessageFieldCodec<int>(attribute),
                TypeCode.UInt32 => new MessageFieldCodec<uint>(attribute),
                TypeCode.Int64  => new MessageFieldCodec<long>(attribute),
                TypeCode.UInt64 => new MessageFieldCodec<ulong>(attribute),
                _ => throw new UnreachableException()
            };

            enumCodec.Encode(writer, Convert.ChangeType(value, Enum.GetUnderlyingType(type)), type);
            return;
        }

        if (typeof(IMessage).IsAssignableFrom(type))
        {
            EncodeMessage(writer, (IMessage)value, type);
            return;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            var enumerableCodec = new CollectionMessageFieldCodec(this, attribute);
            enumerableCodec.Encode(writer, value, type);
            return;
        }

        throw new NotSupportedException($"Type '{type.Name}' is not supported for encoding");
    }

    private object DecodeValue(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, IMessageFieldAttribute attribute)
    {
        if (StandardFieldCodecs.TryGetValue(type, out var serializer))
        {
            return serializer(attribute).Decode(reader, type);
        }

        if (type.IsEnum)
        {
            IMessageFieldCodec enumCodec = Type.GetTypeCode(Enum.GetUnderlyingType(type)) switch
            {
                TypeCode.Char   => new MessageFieldCodec<char>(attribute),
                TypeCode.SByte  => new MessageFieldCodec<sbyte>(attribute),
                TypeCode.Byte   => new MessageFieldCodec<byte>(attribute),
                TypeCode.Int16  => new MessageFieldCodec<short>(attribute),
                TypeCode.UInt16 => new MessageFieldCodec<ushort>(attribute),
                TypeCode.Int32  => new MessageFieldCodec<int>(attribute),
                TypeCode.UInt32 => new MessageFieldCodec<uint>(attribute),
                TypeCode.Int64  => new MessageFieldCodec<long>(attribute),
                TypeCode.UInt64 => new MessageFieldCodec<ulong>(attribute),
                _ => throw new UnreachableException()
            };

            return Enum.ToObject(type, enumCodec.Decode(reader, type));
        }

        if (typeof(IMessage).IsAssignableFrom(type))
        {
            return DecodeMessage(reader, type);
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            var enumerableCodec = new CollectionMessageFieldCodec(this, attribute);
            return enumerableCodec.Decode(reader, type);
        }

        throw new NotSupportedException($"Type '{type.Name}' is not supported for decoding");
    }

    private Type GetEnumerableElementType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type enumerableType)
    {
        var ex = new ArgumentException($"Cannot determine element type for {enumerableType.Name}");

        if (enumerableType.IsArray)
            return enumerableType.GetElementType() ?? throw ex;

        var enumerableInterface = enumerableType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableInterface != null)
            return enumerableInterface.GetGenericArguments()[0] ?? throw ex;

        if (enumerableType.IsGenericType &&
            typeof(IEnumerable).IsAssignableFrom(enumerableType.GetGenericTypeDefinition()))
        {
            return enumerableType.GetGenericArguments()[0] ?? throw ex;
        }

        throw ex;
    }

    private List<MessageMember> GetSerializableMembers(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        var members = new List<MessageMember>();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<MessageFieldAttribute>();
            if (attribute != null)
                members.Add(new MessageMember(field, attribute));
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => p.CanRead || p.CanWrite);

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<MessageFieldAttribute>();
            if (attribute != null)
                members.Add(new MessageMember(property, attribute));
        }

        return members;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private Type? ToSupportedFieldType(Type? type)
    {
        return Type.GetTypeCode(type) switch
        {
            TypeCode.Object   => type != null && _typeSet.TryGetValue(type, out var v) ? v : null,
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
            TypeCode.DateTime => typeof(DateTime),
            TypeCode.String   => typeof(string),
            _ => null
        };
    }

    void IMessageFieldCodec.Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        EncodeValue(writer, value, sourceType, new MessageFieldAttribute(0));
    }

    object IMessageFieldCodec.Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        return DecodeValue(reader, targetType, new MessageFieldAttribute(0));
    }
}
