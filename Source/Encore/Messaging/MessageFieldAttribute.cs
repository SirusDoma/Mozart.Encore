using System.Diagnostics.CodeAnalysis;

namespace Encore.Messaging;

public interface IMessageFieldAttribute
{
    int Order { get; set; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    Type? CodecType { get; }

    object?[]? CodecArgs { get; }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class MessageFieldAttribute : Attribute, IMessageFieldAttribute
{
    public int Order { get; set; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? CodecType { get; }

    public object?[]? CodecArgs { get; }

    public MessageFieldAttribute(int order = 0)
    {
        Order = order;
    }

    public MessageFieldAttribute(
        int order, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type codecType)
    {
        if (codecType != null && !typeof(MessageFieldCodec).IsAssignableFrom(codecType))
            throw new ArgumentException($"Codec type must implement {nameof(MessageFieldCodec)}", nameof(codecType));
            
        Order = order;
        CodecType = codecType;
        CodecArgs = null;
    }

    public MessageFieldAttribute(
        int order,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type codecType,
        params object?[] args
    )
    {
        if (codecType != null && !typeof(MessageFieldCodec).IsAssignableFrom(codecType))
            throw new ArgumentException($"Codec type must implement {nameof(MessageFieldCodec)}", nameof(codecType));
            
        Order = order;
        CodecType = codecType;
        CodecArgs = args;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class MessageFieldAttribute<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCodec
> : MessageFieldAttribute
    where TCodec : MessageFieldCodec
{
    public MessageFieldAttribute(int order = 0, params object[] args) :
        base(order, typeof(TCodec), args)
    {
    }
}

