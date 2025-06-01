using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Encore.Messaging;

public partial class DefaultMessageCodec
{
    private class MessageMember
    {
        private readonly FieldInfo? _field;
        private readonly PropertyInfo? _property;

        public MessageFieldAttribute Attribute { get; }

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public Type MemberType { get; }

        public string Name { get; }

        public bool CanRead  => _property?.CanRead ?? true;
        public bool CanWrite => _property?.CanWrite ?? true;

        public MessageMember(FieldInfo field, MessageFieldAttribute attribute)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _property = null;

            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            MemberType = field.FieldType;
            Name = field.Name;
        }

        public MessageMember(PropertyInfo property, MessageFieldAttribute attribute)
        {
            _field = null;
            _property = property ?? throw new ArgumentNullException(nameof(property));

            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            MemberType = property.PropertyType;
            Name = property.Name;
        }

        public object? GetValue(object instance)
        {
            if (_field != null)
                return _field.GetValue(instance);

            if (_property != null)
                return _property.GetValue(instance);

            throw new InvalidOperationException("Invalid member type");
        }

        public void SetValue(object instance, object? value)
        {
            if (_field != null)
            {
                _field.SetValue(instance, value);
                return;
            }

            if (_property != null)
            {
                _property.SetValue(instance, value);
                return;
            }

            throw new InvalidOperationException("Invalid member type");
        }

        public MessageFieldCodec? CreateFieldCodec(DefaultMessageCodec codec)
        {
            if (Attribute.CodecType == null)
                return null;

            if (Attribute.CodecType.IsAssignableTo(typeof(CollectionMessageFieldCodec)))
                return new CollectionMessageFieldCodec(codec, Attribute);

            try
            {
                object?[] args = Attribute.CodecArgs ?? [];
                return (MessageFieldCodec)Activator.CreateInstance(Attribute.CodecType, args.Prepend(Attribute).ToArray())!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create codec of type '{Attribute.CodecType.Name}' with provided arguments", ex);
            }
        }
    }
}