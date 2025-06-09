namespace Encore.Messaging;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class CollectionMessageFieldAttribute : MessageFieldAttribute
{
    public TypeCode PrefixSizeType { get; }

    public int MinCount { get; }

    public int MaxCount { get; }

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
