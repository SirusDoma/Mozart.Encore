using System.Diagnostics.CodeAnalysis;

namespace Encore.Messaging;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CollectionMessageFieldAttribute : MessageFieldAttribute
{
    public TypeCode PrefixSizeType { get; }

    public int MinCount { get; }

    public int MaxCount { get; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? ElementCodecType { get; protected init; }

    public object?[]? ElementCodecArgs { get; protected init; }

    public CollectionMessageFieldAttribute(
        int order,
        TypeCode prefixSizeType = TypeCode.Empty,
        int minCount = 0,
        int maxCount = ushort.MaxValue
    ) : base(order, typeof(CollectionMessageFieldCodec))
    {
        PrefixSizeType   = prefixSizeType;
        MinCount         = minCount;
        MaxCount         = maxCount;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class CollectionMessageFieldAttribute<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCodec
> : CollectionMessageFieldAttribute
    where TCodec : MessageFieldCodec
{
    public CollectionMessageFieldAttribute(
        int order,
        TypeCode prefixSizeType = TypeCode.Empty,
        int minCount = 0,
        int maxCount = ushort.MaxValue,
        params object[] args
    ) : base(order, prefixSizeType, minCount, maxCount)
    {
        ElementCodecType = typeof(TCodec);
        ElementCodecArgs = args;
    }
}
