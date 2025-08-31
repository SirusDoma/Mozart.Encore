using System.Diagnostics.CodeAnalysis;
using Encore.Messaging;

namespace Amadeus.Messages.Codecs;

public class TimeSpanMinuteCodec : MessageFieldCodec
{
    public TimeSpanMinuteCodec(MessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        var expiry = (TimeSpan)value;
        writer.Write((int)expiry.TotalMinutes);
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        throw new NotSupportedException();
    }
}