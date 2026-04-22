using System.Diagnostics.CodeAnalysis;
using Encore.Messaging;

namespace Memoryer.Messages.Codecs;

public class SubscriptionTimeExpiryCodec: MessageFieldCodec
{
    public SubscriptionTimeExpiryCodec(MessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        if (value is int seconds)
        {
            writer.Write(seconds);
            return;
        }

        if (value is not TimeSpan time)
            throw new NotSupportedException();

        writer.Write((int)time.TotalSeconds);
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        throw new NotSupportedException();
    }
}
