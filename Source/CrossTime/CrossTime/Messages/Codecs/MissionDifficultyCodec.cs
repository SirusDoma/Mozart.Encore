using System.Diagnostics.CodeAnalysis;
using Encore.Messaging;
using Encore.Metadata;

namespace CrossTime.Messages.Codecs;

public class MissionDifficultyCodec : MessageFieldCodec
{
    public MissionDifficultyCodec(MessageFieldAttribute attribute)
        : base(attribute)
    {
    }

    public override void Encode(BinaryWriter writer, object value,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type sourceType)
    {
        writer.Write((int)(Difficulty)value + 1);
    }

    public override object Decode(BinaryReader reader,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type targetType)
    {
        return (Difficulty)(reader.ReadInt32() - 1);
    }
}
